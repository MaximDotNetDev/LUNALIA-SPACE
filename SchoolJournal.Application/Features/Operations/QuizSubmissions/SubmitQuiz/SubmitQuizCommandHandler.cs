using System.Text.Json.Nodes;
using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Operations.QuizSubmissions.SubmitQuiz;

public sealed class SubmitQuizCommandHandler(
    IQuizSubmissionRepository submissionRepository,
    IQuizAssignmentRepository assignmentRepository,
    IQuizQuestionRepository questionRepository,
    IStudentRepository studentRepository,
    ICurrentUserService currentUserService,
    IWalletRepository walletRepository)
    : IRequestHandler<SubmitQuizCommand, ErrorOr<SubmitQuizResponse>>
{
    public async Task<ErrorOr<SubmitQuizResponse>> Handle(SubmitQuizCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Ідентифікація учня
        var userId = currentUserService.GetUserId();
        var student = await studentRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);

        if (student is null)
        {
            return Error.Forbidden(
                code: "QuizSubmission.NotAStudent",
                description: "Тільки учні можуть здавати тести.");
        }

        // 2. Перевірка наявності призначення
        var assignment = await assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);
        if (assignment is null)
        {
            return Error.NotFound(
                code: "QuizSubmission.AssignmentNotFound",
                description: "Призначення тесту не знайдено.");
        }

        // 3. Fail Fast: Захист від подвійної здачі
        var alreadySubmitted = await submissionRepository.HasStudentSubmittedAsync(request.AssignmentId, student.StudentId, cancellationToken).ConfigureAwait(false);
        if (alreadySubmitted)
        {
            return Error.Conflict(
                code: "QuizSubmission.AlreadySubmitted",
                description: "Ви вже здавали цей тест раніше.");
        }

        // 4. Отримання питань та перевірка відповідей
        var (questions, _) = await questionRepository.GetPagedByQuizIdAsync(assignment.QuizId, 0, 1000, cancellationToken).ConfigureAwait(false);

        int finalScore = 0;
        int maxScore = 0;

        foreach (var question in questions)
        {
            maxScore += question.Points;

            var userAnswerDto = request.Answers.FirstOrDefault(a => a.QuestionId == question.QuestionId);
            // Використовуємо точну назву властивості з твого QuizAnswerDto
            var userAnswer = userAnswerDto?.AnswerJson ?? string.Empty;

            // Тепер метод сам вираховує зароблені бали, враховуючи тип питання (Fail Fast)
            finalScore += EvaluateAnswer(userAnswer, question.ContentJson, question.Points, question.QuestionType);
        }

        // 5. Збереження результату
        var submission = new QuizSubmission
        {
            AssignmentId = request.AssignmentId,
            StudentId = student.StudentId,
            Score = finalScore,
            MaxScore = maxScore,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var submissionId = await submissionRepository.AddAsync(submission, cancellationToken).ConfigureAwait(false);

        // --- ІНТЕГРАЦІЯ LUNAR COINS (Правило 1: 1 бал = 1 монета) ---
        if (finalScore > 0)
        {
            var subjectId = await assignmentRepository.GetSubjectIdByAssignmentAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);
            var wallet = await walletRepository.GetWalletAsync(student.StudentId, subjectId, cancellationToken).ConfigureAwait(false);
            Guid targetWalletId;

            if (wallet is null)
            {
                targetWalletId = await walletRepository.CreateWalletAsync(new Wallet
                {
                    StudentId = student.StudentId,
                    SubjectId = subjectId,
                    Balance = finalScore
                }, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                targetWalletId = wallet.WalletId;
                await walletRepository.UpdateBalanceAsync(wallet.WalletId, wallet.Balance + finalScore, [.. wallet.RowVersion], cancellationToken).ConfigureAwait(false);
            }

            await walletRepository.RecordTransactionAsync(new CoinTransaction
            {
                WalletId = targetWalletId,
                Amount = finalScore,
                ReferenceId = submissionId,
                TransactionType = "Earned_Quiz"
            }, cancellationToken).ConfigureAwait(false);
        }

        return new SubmitQuizResponse(
            submissionId,
            finalScore,
            maxScore,
            submission.CreatedAt.UtcDateTime,
            finalScore); // EarnedCoins = finalScore
        
    }

    private static int EvaluateAnswer(string userAnswer, string contentJson, int maxPoints, int questionType)
    {
        if (string.IsNullOrWhiteSpace(userAnswer) || string.IsNullOrWhiteSpace(contentJson)) return 0;

        try
        {
            var content = System.Text.Json.Nodes.JsonNode.Parse(contentJson);
            if (content is null) return 0;

            // CQRS & SRP: Жорстка маршрутизація перевірки за типом питання
            if (questionType == 9 && content["words"] is System.Text.Json.Nodes.JsonArray crosswordWords)
            {
                return EvaluateCrosswordAnswer(userAnswer, crosswordWords, maxPoints);
            }

            if (questionType == 10 && content["words"] is System.Text.Json.Nodes.JsonArray)
            {
                // Передаємо ВЕСЬ content, щоб мати доступ до масиву "cells" (самої сітки літер)
                return EvaluateFillwordAnswer(userAnswer, content, maxPoints);
            }

            if (questionType == 4 && content["isTrue"] is not null)
            {
                return EvaluateTrueFalseAnswer(userAnswer, content, maxPoints);
            }

            // Звичайні тести
            return EvaluateStandardAnswer(userAnswer, content, maxPoints);
        }
        catch (System.Text.Json.JsonException)
        {
            return 0;
        }
    }

    private static int EvaluateFillwordAnswer(string userAnswer, System.Text.Json.Nodes.JsonNode content, int maxPoints)
    {
        if (System.Text.Json.Nodes.JsonNode.Parse(userAnswer) is not System.Text.Json.Nodes.JsonArray userCoordsArray) return 0;
        if (content["words"] is not System.Text.Json.Nodes.JsonArray wordsArray || wordsArray.Count == 0) return 0;

        var userCoords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in userCoordsArray)
        {
            if (item?.GetValue<string>() is { Length: > 0 } coord) userCoords.Add(coord);
        }

        // 1. Відновлюємо мапу сітки. Це дозволить учню виділяти слово БУДЬ-ЯКИМ валідним шляхом.
        var grid = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (content["cells"] is System.Text.Json.Nodes.JsonArray cellsArray)
        {
            foreach (var cellNode in cellsArray)
            {
                int r = Convert.ToInt32(cellNode?["row"]?.ToString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                int c = Convert.ToInt32(cellNode?["column"]?.ToString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                grid[$"{r}_{c}"] = cellNode?["letter"]?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
            }
        }
        else
        {
            // Fallback для старих записів: будуємо сітку з жорстких шляхів
            foreach (var wNode in wordsArray)
            {
                string ans = wNode?["answer"]?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
                if (wNode?["path"] is System.Text.Json.Nodes.JsonArray pArray && ans.Length == pArray.Count)
                {
                    for (int i = 0; i < ans.Length; i++)
                    {
                        int r = Convert.ToInt32(pArray[i]?["row"]?.ToString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                        int c = Convert.ToInt32(pArray[i]?["column"]?.ToString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                        grid[$"{r}_{c}"] = ans[i].ToString();
                    }
                }
            }
        }

        int foundWords = 0;
        var usedValidCoords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 2. DFS-пошук на графах
        foreach (var wordNode in wordsArray)
        {
            string expectedWord = wordNode?["answer"]?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrEmpty(expectedWord)) continue;

            var pathFound = FindWordPathDfs(expectedWord, userCoords, grid);
            if (pathFound.Count > 0)
            {
                foundWords++;
                foreach (var p in pathFound) usedValidCoords.Add(p);
            }
        }

        // 3. FAIL FAST: Штрафуємо лише за ті клітинки, які не увійшли до ЖОДНОГО знайденого слова.
        int extraSelections = userCoords.Count(c => !usedValidCoords.Contains(c));
        double pointPerWord = (double)maxPoints / wordsArray.Count;

        // Штраф: кожна зайва клітинка знімає половину вартості одного слова
        double rawScore = (foundWords * pointPerWord) - (extraSelections * (pointPerWord / 2.0));

        return rawScore < 0 ? 0 : (int)Math.Floor(rawScore);
    }

    private static List<string> FindWordPathDfs(string word, HashSet<string> userCoords, Dictionary<string, string> grid)
    {
        // Nullable Fix: TryGetValue має атрибут [MaybeNullWhen(false)], тому безпечно використовувати string?
        foreach (var startCoord in userCoords.Where(c => grid.TryGetValue(c, out string? letter) && string.Equals(letter, word[0].ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            var path = new List<string>();
            if (ExploreDfs(startCoord, 0, word, userCoords, grid, new HashSet<string>(StringComparer.OrdinalIgnoreCase), path))
            {
                return path;
            }
        }
        return [];
    }

    private static bool ExploreDfs(string current, int index, string word, HashSet<string> userCoords, Dictionary<string, string> grid, HashSet<string> visited, List<string> path)
    {
        visited.Add(current);
        path.Add(current);

        if (index == word.Length - 1) return true;

        var parts = current.Split('_');
        if (parts.Length == 2 && int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
        {
            string[] neighbors = { $"{r + 1}_{c}", $"{r - 1}_{c}", $"{r}_{c + 1}", $"{r}_{c - 1}" };

            // LINQ Any: замінюємо foreach з одним if на Any() для спрощення. Nullable Fix: out string?
            if (neighbors
                .Where(n => !visited.Contains(n) && userCoords.Contains(n) && grid.TryGetValue(n, out string? neighborLetter) && string.Equals(neighborLetter, word[index + 1].ToString(), StringComparison.OrdinalIgnoreCase))
                .Any(n => ExploreDfs(n, index + 1, word, userCoords, grid, visited, path)))
            {
                return true;
            }
        }

        visited.Remove(current);
        path.RemoveAt(path.Count - 1);
        return false;
    }

    private static int EvaluateTrueFalseAnswer(string userAnswer, System.Text.Json.Nodes.JsonNode content, int maxPoints)
    {
        bool expectedIsTrue = content["isTrue"]?.GetValue<bool>() ?? false;

        string normalizedUserAnswer = userAnswer.Trim().ToUpperInvariant();

        // Захист: якщо учень обійшов UI і надіслав порожній рядок або щось інше
        if (normalizedUserAnswer != "ТАК" && normalizedUserAnswer != "НІ")
        {
            return 0;
        }

        bool userIsTrue = normalizedUserAnswer == "ТАК";

        return expectedIsTrue == userIsTrue ? maxPoints : 0;
    }

    private static int EvaluateCrosswordAnswer(string userAnswer, System.Text.Json.Nodes.JsonArray wordsArray, int maxPoints)
    {
        if (System.Text.Json.Nodes.JsonNode.Parse(userAnswer) is not System.Text.Json.Nodes.JsonObject userAnswersObj)
        {
            return 0;
        }

        int totalWords = wordsArray.Count;
        if (totalWords == 0) return 0;

        int correctWordsCount = wordsArray.Count(wordNode => IsCrosswordWordCorrect(wordNode, userAnswersObj));

        double pointPerWord = (double)maxPoints / totalWords;
        return (int)Math.Round(correctWordsCount * pointPerWord, MidpointRounding.AwayFromZero);
    }

    private static bool IsCrosswordWordCorrect(System.Text.Json.Nodes.JsonNode? wordNode, System.Text.Json.Nodes.JsonObject userAnswersObj)
    {
        string answer = wordNode?["answer"]?.GetValue<string>()?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(answer)) return false;

        int startRow = Convert.ToInt32(wordNode?["row"]?.ToString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);
        int startCol = Convert.ToInt32(wordNode?["column"]?.ToString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);

        string horizStr = wordNode?["isHorizontal"]?.ToString() ?? "true";
        bool isHorizontal = horizStr.Equals("true", StringComparison.OrdinalIgnoreCase) || horizStr == "1";

        for (int i = 0; i < answer.Length; i++)
        {
            // Пропускаємо перевірку символів-розділювачів (пробіл, дефіс), оскільки вони є "невидимими" клітинками
            if (answer[i] == ' ' || answer[i] == '-') continue;

            int row = isHorizontal ? startRow : startRow + i;
            int col = isHorizontal ? startCol + i : startCol;

            string expectedLetter = NormalizeLetterLayout(answer[i].ToString().ToUpperInvariant());
            string key = $"{row}_{col}";

            var userLetterNode = userAnswersObj[key];
            if (userLetterNode is null) return false;

            string userLetter = NormalizeLetterLayout((userLetterNode.GetValue<string>() ?? string.Empty).Trim().ToUpperInvariant());

            if (!string.Equals(userLetter, expectedLetter, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeLetterLayout(string letter)
    {
        return letter.Replace("I", "І", StringComparison.Ordinal)
                     .Replace("O", "О", StringComparison.Ordinal)
                     .Replace("A", "А", StringComparison.Ordinal)
                     .Replace("C", "С", StringComparison.Ordinal)
                     .Replace("E", "Е", StringComparison.Ordinal)
                     .Replace("P", "Р", StringComparison.Ordinal)
                     .Replace("T", "Т", StringComparison.Ordinal)
                     .Replace("X", "Х", StringComparison.Ordinal);
    }

    private static int EvaluateStandardAnswer(string userAnswer, System.Text.Json.Nodes.JsonNode content, int maxPoints)
    {
        var correctAnswerString = content["correctAnswer"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(correctAnswerString)) return 0;

        var correctKeys = correctAnswerString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        var userKeys = userAnswer
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        bool isCorrect = correctKeys.SequenceEqual(userKeys, StringComparer.OrdinalIgnoreCase);
        return isCorrect ? maxPoints : 0;
    }
}