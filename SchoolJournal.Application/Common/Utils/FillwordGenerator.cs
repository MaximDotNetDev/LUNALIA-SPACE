using System;
using System.Collections.Generic;
using System.Linq;
using SchoolJournal.Contracts.DTOs.AiGeneration;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions.Content;

namespace SchoolJournal.Application.Common.Utils;

public static class FillwordGenerator
{
    private static readonly char[] s_cyrillicAlphabet = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ".ToCharArray();

    public static FillwordContentDto Generate(IReadOnlyCollection<FillwordAiWordDto> inputWords, int targetWordCount)
    {
        if (inputWords is null || inputWords.Count == 0) return new FillwordContentDto(0, 0, [], []);

        var wordsToPlace = inputWords
            .Select(w => new { Answer = w.Answer.ToUpperInvariant().Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal), w.Clue })
            .Where(w => w.Answer.Length is >= 3 and <= 12)
            .Take(targetWordCount)
            .ToList();

        if (wordsToPlace.Count == 0) return new FillwordContentDto(0, 0, [], []);

        // Вираховуємо оптимальний розмір квадрата (щільність слів ~70%)
        int totalLetters = wordsToPlace.Sum(w => w.Answer.Length);
        int gridSize = (int)Math.Max(7, Math.Ceiling(Math.Sqrt(totalLetters * 1.5)));
        gridSize = Math.Min(gridSize, 15); // Обмежуємо максимальний розмір 15x15 для дітей

        char[][] grid = new char[gridSize][];
        for (int i = 0; i < gridSize; i++)
        {
            grid[i] = new char[gridSize];
            Array.Fill(grid[i], '\0');
        }

        var finalWords = new List<FillwordWordDto>();
        int wordNumber = 1;

        foreach (var word in wordsToPlace)
        {
            var path = TryPlaceWord(grid, word.Answer, gridSize);
            if (path is null) continue; // Пропускаємо слово, якщо воно не влізло

            finalWords.Add(new FillwordWordDto(wordNumber++, word.Answer, word.Clue, path));
        }

        var finalCells = new List<FillwordCellDto>();

        // Заповнюємо порожнечі використовуючи криптографічно безпечний генератор (Fix SonarAnalyzer)
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                char letter = grid[r][c];
                bool isRandom = false;

                if (letter == '\0')
                {
                    int randomIndex = System.Security.Cryptography.RandomNumberGenerator.GetInt32(s_cyrillicAlphabet.Length);
                    letter = s_cyrillicAlphabet[randomIndex];
                    isRandom = true;
                }

                finalCells.Add(new FillwordCellDto(r, c, letter, isRandom));
            }
        }

        return new FillwordContentDto(gridSize, gridSize, finalWords, finalCells);
    }

    private static List<FillwordCoordinateDto>? TryPlaceWord(char[][] grid, string word, int gridSize)
    {
        var startPositions = new List<(int r, int c)>();

        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                // Сувора ізоляція: слово може починатися ТІЛЬКИ з абсолютно порожньої клітинки
                if (grid[r][c] == '\0')
                    startPositions.Add((r, c));

        // Рандомізуємо точки старту безпечно
        startPositions = startPositions.OrderBy(_ => Guid.NewGuid()).ToList();

        foreach (var (startR, startC) in startPositions)
        {
            // Використовуємо Jagged Array (масив масивів) замість Multidimensional для оптимізації CLR
            var visited = new bool[gridSize][];
            for (int i = 0; i < gridSize; i++) visited[i] = new bool[gridSize];

            var path = new List<FillwordCoordinateDto>();
            
            if (DFS(grid, word, 0, startR, startC, gridSize, visited, path))
            {
                foreach (var coord in path)
                {
                    grid[coord.Row][coord.Column] = word[path.IndexOf(coord)];
                }
                return path;
            }
        }

        return null;
    }

    private static bool DFS(char[][] grid, string word, int charIndex, int r, int c, int gridSize, bool[][] visited, List<FillwordCoordinateDto> path)
    {
        if (charIndex == word.Length) return true;

        if (r < 0 || r >= gridSize || c < 0 || c >= gridSize) return false;
        if (visited[r][c]) return false;
        // Сувора ізоляція: якщо клітинка вже зайнята будь-якою літерою іншого слова, шлях заборонено
        if (grid[r][c] != '\0') return false;

        visited[r][c] = true;
        path.Add(new FillwordCoordinateDto(r, c));

        // FAIL FAST для дітей: Дозволяємо рух ТІЛЬКИ вправо (0, 1) та вниз (1, 0)
        var moves = new List<(int dr, int dc)> { (0, 1), (1, 0) }
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        foreach (var (dr, dc) in moves)
        {
            if (DFS(grid, word, charIndex + 1, r + dr, c + dc, gridSize, visited, path))
            {
                return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        visited[r][c] = false;
        return false;
    }
}