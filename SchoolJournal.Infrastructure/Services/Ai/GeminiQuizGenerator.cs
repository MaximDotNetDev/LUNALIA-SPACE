using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErrorOr;
using Microsoft.Extensions.Options;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Infrastructure.Common.Options;
using SchoolJournal.Infrastructure.Services.Ai.Models;
using SchoolJournal.Contracts.DTOs.AiGeneration;

namespace SchoolJournal.Infrastructure.Services.Ai;

public sealed class GeminiQuizGenerator : IAiQuizGenerator
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public GeminiQuizGenerator(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = _options.BaseUrl;
    }

    public async Task<ErrorOr<GeneratedQuizResponse>> GenerateMultipleChoiceAsync(
            string text, int totalQuestions, int multiAnswerCount, int judgmentCount, int pointsPerQuestion, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Ти — професійний вчитель. Створи тест на {{totalQuestions}} питань на основі фактів з наступного тексту.
            Текст: {{text}}

            ВИМОГИ ДО ФОРМАТУВАННЯ ТА ЗМІСТУ (ВИКОНУВАТИ СУВОРО):
            1. Загальна кількість питань: рівно {{totalQuestions}}.
            2. Зроби так, щоб рівно {{multiAnswerCount}} питань мали ДЕКІЛЬКА правильних відповідей.
            3. Зроби так, щоб рівно {{judgmentCount}} питань були формату "Аналіз суджень" (Хто правий?).
               - Опиши ситуацію, де двоє дітей (обов'язково використовуй як хлопчиків, так і дівчаток) висловлюють різні твердження щодо теми.
               - Варіанти відповідей мають бути адаптовані до персонажів: ["а) правий учень", "б) права учениця", "в) обоє праві", "г) обоє не праві"].
               - Поле "questionType" має містити СУВОРО цифру 1.
            4. Для всіх ІНШИХ звичайних питань поле "questionType" має містити СУВОРО цифру 0.
            5. Кожне питання оцінюється в {{pointsPerQuestion}} балів (поле "points").
            6. КІЛЬКІСТЬ ВАРІАНТІВ ВІДПОВІДЕЙ:
               - Для звичайних питань або питань на судження: рівно 4 варіанти (а, б, в, г).
               - Якщо правильних відповідей декілька: рівно 5 варіантів (а, б, в, г, д).
            7. Формулюй питання як самостійні! КАТЕГОРИЧНО ЗАБОРОНЕНО використовувати фрази типу "згідно з текстом". 
            8. Правильні відповіді мають містити ТІЛЬКИ літери правильних варіантів з дужкою. Якщо їх декілька, перелічи через кому з пробілом.
            9. ФОРМУЛИ ТА РІВНЯННЯ: Для математики та хімії ОБОВ'ЯЗКОВО використовуй Юнікод-символи (x², H₂O).
            10. НАЗВА ТЕСТУ: Для поля "title" автоматично визнач головну тему тексту або назву параграфа. Не використовуй шаблонні слова.

            Поверни результат СУВОРО у форматі JSON. Не пиши жодних вступних слів.
            Шаблон:
            {
              "title": "Тест: [Реальна тема параграфа з тексту]",
              "questions": [
                {
                  "orderIndex": 0,
                  "questionText": "1. Текст питання (без згадок про текст)",
                  "questionType": 0,
                  "contentJson": "{\"options\":[\"а) Варіант 1\",\"б) Варіант 2\",\"в) Варіант 3\",\"г) Варіант 4\"],\"correctAnswer\":\"а)\"}",
                  "points": {{pointsPerQuestion}}
                }
              ]
            }
            ВАЖЛИВО: Значення contentJson має бути СЕРІАЛІЗОВАНИМ РЯДКОМ (Escaped JSON string), а не об'єктом.
            """;

        return await SendRequestInternalAsync<GeneratedQuizResponse>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<GeneratedQuizResponse>> GenerateFillInTheBlankAsync(
        string text, int questionCount, int pointsPerQuestion, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Створи завдання "Вставте пропущені слова" на {{questionCount}} питань за текстом: {{text}}
            Повернути СУВОРО JSON об'єкт за шаблоном:
            {
              "title": "Пропущені слова: Тема",
              "questions": [
                {
                  "orderIndex": 0,
                  "questionText": "Вставте слово",
                  "questionType": 2,
                  "contentJson": "{\"textWithBlanks\":\"Текст [____] Текст\",\"missingWords\":[\"Слово\"]}",
                  "points": {{pointsPerQuestion}}
                }
              ]
            }
            ВАЖЛИВО: Значення contentJson має бути СЕРІАЛІЗОВАНИМ РЯДКОМ (Escaped JSON string).
            """;

        return await SendRequestInternalAsync<GeneratedQuizResponse>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<GeneratedQuizResponse>> GenerateMatchingAsync(
        string text, int questionCount, int pointsPerQuestion, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Створи завдання "Встановіть відповідність" на {{questionCount}} питань за текстом: {{text}}
            Повернути СУВОРО JSON об'єкт за шаблоном:
            {
              "title": "Відповідність: Тема",
              "questions": [
                {
                  "orderIndex": 0,
                  "questionText": "Встановіть відповідність",
                  "questionType": 3,
                  "contentJson": "{\"leftOptions\":[\"1.А\"],\"shuffledRightOptions\":[\"а)Б\"],\"correctAnswerMap\":\"1-а\"}",
                  "points": {{pointsPerQuestion}}
                }
              ]
            }
            """;

        return await SendRequestInternalAsync<GeneratedQuizResponse>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<TrueFalseAiResponseDto>> GenerateTrueFalseAsync(
            string text, int questionCount, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Ти — професійний вчитель. Створи {{questionCount}} завдань формату "Так чи ні" на основі фактів з тексту.
            Текст: {{text}}

            ВИМОГИ ДО ФОРМАТУВАННЯ ТА ЗМІСТУ (ВИКОНУВАТИ СУВОРО):
            1. Загальна кількість питань: рівно {{questionCount}}.
            2. ФОРМАТ: Питання має бути стверджувальним реченням ("statement"). Учень має визначити, правдиве воно чи хибне.
            3. КАТЕГОРИЧНА ЗАБОРОНА: Це НЕ тест із вибором варіантів. Заборонено створювати варіанти відповідей (А, Б, В тощо).
            4. ПОЯСНЕННЯ: ВАЖЛИВО! Для КОЖНОГО питання ОБОВ'ЯЗКОВО напиши детальний текст у поле "explanation", який аргументує, чому це твердження є правдою (true) чи хибою (false) на основі тексту.
            5. НАЗВА ТЕСТУ: Для поля "title" автоматично визнач головну тему тексту або назву параграфа. Не використовуй шаблонні слова.

            Поверни результат СУВОРО у форматі JSON. Не пиши жодних вступних слів.
            Шаблон:
            {
              "title": "Так чи ні: [Реальна тема параграфа з тексту]",
              "questions": [
                {
                  "statement": "Текст твердження (без згадок про текст)",
                  "isTrue": true,
                  "explanation": "Детальне пояснення..."
                }
              ]
            }
            """;

        return await SendRequestInternalAsync<TrueFalseAiResponseDto>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<GeneratedQuizResponse>> GenerateOddOneOutAsync(
        string text, int questionCount, int pointsPerQuestion, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Створи {{questionCount}} завдань формату "Знайди зайве" за текстом: {{text}}
            Повернути СУВОРО JSON об'єкт за шаблоном:
            {
              "title": "Хто зайвий: Тема",
              "questions": [
                {
                  "orderIndex": 0,
                  "questionText": "Знайдіть зайвий термін.",
                  "questionType": 5,
                  "contentJson": "{\"options\":[\"Термін 1\",\"Термін 2\",\"Термін 3\",\"Термін 4\"],\"oddOneOut\":\"Термін 3\",\"explanation\":\"Термін 3 — це ..., тоді як інші — це ...\"}",
                  "points": {{pointsPerQuestion}}
                }
              ]
            }
            ВАЖЛИВО: Значення contentJson має бути СЕРІАЛІЗОВАНИМ РЯДКОМ.
            """;
        return await SendRequestInternalAsync<GeneratedQuizResponse>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<GeneratedQuizResponse>> GenerateGuessByDescriptionAsync(
        string text, int questionCount, int pointsPerQuestion, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Створи {{questionCount}} завдань формату "Відгадай за описом" за текстом: {{text}}
            Повернути СУВОРО JSON об'єкт за шаблоном:
            {
              "title": "Відгадай за описом: Тема",
              "questions": [
                {
                  "orderIndex": 0,
                  "questionText": "Прочитайте опис і відгадайте зашифрований термін.",
                  "questionType": 6,
                  "contentJson": "{\"description\":\"Текст опису...\",\"correctAnswer\":\"Термін\"}",
                  "points": {{pointsPerQuestion}}
                }
              ]
            }
            ВАЖЛИВО: Значення contentJson має бути СЕРІАЛІЗОВАНИМ РЯДКОМ.
            """;
        return await SendRequestInternalAsync<GeneratedQuizResponse>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<GeneratedQuizResponse>> GenerateProofreaderAsync(
        string text, int questionCount, int pointsPerQuestion, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Створи {{questionCount}} завдань формату "Коректор". Напиши абзац на основі тексту з 2-3 термінологічними помилками. Текст: {{text}}
            Повернути СУВОРО JSON об'єкт за шаблоном:
            {
              "title": "Коректор: Тема",
              "questions": [
                {
                  "orderIndex": 0,
                  "questionText": "Знайдіть помилки в тексті.",
                  "questionType": 7,
                  "contentJson": "{\"textWithErrors\":\"Текст із помилками...\",\"correctText\":\"Правильний текст...\",\"explanation\":\"❌ помилка ➡️ ✅ правильне\"}",
                  "points": {{pointsPerQuestion}}
                }
              ]
            }
            ВАЖЛИВО: Значення contentJson має бути СЕРІАЛІЗОВАНИМ РЯДКОМ.
            """;
        return await SendRequestInternalAsync<GeneratedQuizResponse>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<GeneratedQuizResponse>> GenerateAssociativeBushAsync(
        string text, int questionCount, int pointsPerQuestion, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
            Створи {{questionCount}} завдань формату "Асоціативний кущ". Вибери {{questionCount}} найголовніших слів з тексту (одне слово на завдання). Текст: {{text}}
            Повернути СУВОРО JSON об'єкт за шаблоном:
            {
              "title": "Асоціативний кущ",
              "questions": [
                {
                  "orderIndex": 0,
                  "questionText": "Напишіть свої асоціації до слова: Слово1",
                  "questionType": 8,
                  "contentJson": "{\"mainWord\":\"Слово1\",\"expectedAssociations\":[\"\",\"\",\"\",\"\",\"\"]}",
                  "points": {{pointsPerQuestion}}
                }
              ]
            }
            ВАЖЛИВО: Значення contentJson має бути СЕРІАЛІЗОВАНИМ РЯДКОМ.
            """;
        return await SendRequestInternalAsync<GeneratedQuizResponse>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<CrosswordAiResponseDto>> GenerateCrosswordAsync(
                string text, int wordCount, CancellationToken cancellationToken = default)
    {
        int requestCount = wordCount + 5;

        var prompt = $$"""
            Ти — професійний укладач кросвордів. Прочитай текст і виділи з нього {{requestCount}} ключових термінів.
            Текст: {{text}}

            ВИМОГИ ДО ФОРМАТУВАННЯ ТА ЗМІСТУ (ВИКОНУВАТИ СУВОРО):
            1. Назва кросворду: автоматично визнач тему параграфа для поля "title" (наприклад: "Кросворд: Будова клітини").
            2. Вимоги до слова ("answer"): 
            - Може бути одне слово або коротка фраза (до 2-3 слів, наприклад "БАЗА ДАНИХ").
            - ТІЛЬКИ ВЕЛИКІ ЛІТЕРИ.
            - Дозволені пробіли між словами, але жодних інших спецсимволів чи дефісів.
            - Слово/фраза ОБОВ'ЯЗКОВО має бути в НАЗИВНОМУ ВІДМІНКУ (хто? що?).
            3. Вимоги до загадки ("clue"): 
               - Чітка та зрозуміла загадка або опис до цього слова на основі тексту.
            4. Повернути СУВОРО JSON об'єкт за шаблоном. Не пиши жодних вступних слів.
            
            Шаблон результату:
            {
              "title": "Кросворд: [Реальна тема параграфа з тексту]",
              "words": [
                {
                  "answer": "ТЕРМІН",
                  "clue": "Чітка загадка або опис терміну"
                }
              ]
            }
            """;

        return await SendRequestInternalAsync<CrosswordAiResponseDto>(prompt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ErrorOr<FillwordAiResponseDto>> GenerateFillwordAsync(
            string text, int wordCount, CancellationToken cancellationToken = default)
    {
        int requestCount = wordCount + 3; // Запас на випадок, якщо алгоритм не зможе згорнути якесь слово

        var prompt = $$"""
            Ти — укладач дитячих філвордів (угорських кросвордів). Прочитай текст і виділи з нього {{requestCount}} ключових термінів.
            Текст: {{text}}

            ВИМОГИ ДО ФОРМАТУВАННЯ ТА ЗМІСТУ (ВИКОНУВАТИ СУВОРО):
            1. Назва філворду: автоматично визнач тему параграфа для поля "title".
            2. Вимоги до слова ("answer"): 
               - СУВОРО ОДНЕ СЛОВО (довжина від 3 до 12 літер).
               - ТІЛЬКИ ВЕЛИКІ ЛІТЕРИ українського алфавіту.
               - ЖОДНИХ пробілів, дефісів чи апострофів.
               - Слово ОБОВ'ЯЗКОВО має бути в НАЗИВНОМУ ВІДМІНКУ (хто? що?).
            3. Вимоги до загадки ("clue"): 
               - Чітка та зрозуміла загадка або опис до цього слова.
            4. Повернути СУВОРО JSON об'єкт за шаблоном. Не пиши жодних вступних слів.

            Шаблон результату:
            {
              "title": "Філворд: [Реальна тема параграфа з тексту]",
              "words": [
                {
                  "answer": "ТЕРМІН",
                  "clue": "Загадка або опис терміну"
                }
              ]
            }
            """;

        return await SendRequestInternalAsync<FillwordAiResponseDto>(prompt, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ErrorOr<T>> SendRequestInternalAsync<T>(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = CreateRequestBody(prompt);
        var url = $"v1beta/models/gemini-2.5-flash:generateContent?key={_options.ApiKey}";
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken).ConfigureAwait(false);

                var result = await ProcessResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);

                if (result.IsError && result.FirstError.Code == "Ai.Overloaded" && attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                if (attempt == maxRetries) return Error.Unexpected(code: "Ai.NetworkError", description: $"Мережева помилка: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken).ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                return Error.Unexpected(code: "Ai.ParsingError", description: $"Помилка парсингу JSON: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                return Error.Unexpected(code: "Ai.Timeout", description: $"Час очікування вичерпано: {ex.Message}");
            }
        }

        return Error.Unexpected(code: "Ai.Unknown", description: "Невідома помилка під час запиту до ШІ.");
    }

    private static object CreateRequestBody(string prompt)
    {
        return new
        {
            contents = (object[])[new { parts = (object[])[new { text = prompt }] }],
            generationConfig = new { response_mime_type = "application/json" }
        };
    }

    private static async Task<ErrorOr<T>> ProcessResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests || response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            return Error.Failure(code: "Ai.Overloaded", description: "Сервери ШІ перевантажені. Спробуйте пізніше.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Error.Failure(code: "Ai.RequestFailed", description: $"Помилка виклику API ШІ: {response.ReasonPhrase}");
        }

        var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, ct).ConfigureAwait(false);
        var rawJson = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return Error.Unexpected(code: "Ai.EmptyResponse", description: "Шорт-лист відповідей від ШІ порожній.");
        }

        var cleanedJson = CleanMarkdownJson(rawJson);
        var result = JsonSerializer.Deserialize<T>(cleanedJson, JsonOptions);

        if (result is null)
        {
            return Error.Unexpected(code: "Ai.ParsingError", description: "Не вдалося десеріалізувати структуру тесту.");
        }

        return result;
    }

    private static string CleanMarkdownJson(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return string.Empty;

        string stripped = StripMarkdownTags(rawJson.Trim());

        // Виправлення: Використовуємо StringComparison.Ordinal для безпечного пошуку
        int startIndex = stripped.IndexOf('{', StringComparison.Ordinal);
        if (startIndex == -1) return stripped;

        int endIndex = FindJsonEndIndex(stripped, startIndex);

        return endIndex != -1
            ? stripped.Substring(startIndex, endIndex - startIndex + 1)
            : stripped;
    }

    private static string StripMarkdownTags(string input)
    {
        if (input.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            input = input[7..];
        }
        else if (input.StartsWith("```", StringComparison.Ordinal))
        {
            input = input[3..];
        }

        if (input.EndsWith("```", StringComparison.Ordinal))
        {
            input = input[..^3];
        }

        return input.Trim();
    }

    private static int FindJsonEndIndex(string input, int startIndex)
    {
        int depth = 0;
        bool inQuotes = false;
        bool isEscaped = false;

        for (int i = startIndex; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuotes)
            {
                HandleEscapeState(c, ref inQuotes, ref isEscaped);
                continue;
            }

            // Обробка поза лапками
            if (c == '"') inQuotes = true;
            else if (c == '{') depth++;
            else if (c == '}') depth--;

            if (depth == 0) return i;
        }

        return -1;
    }

    private static void HandleEscapeState(char c, ref bool inQuotes, ref bool isEscaped)
    {
        if (isEscaped)
        {
            isEscaped = false;
        }
        else if (c == '\\')
        {
            isEscaped = true;
        }
        else if (c == '"')
        {
            inQuotes = false;
        }
    }
}