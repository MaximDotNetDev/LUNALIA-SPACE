using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SchoolJournal.Contracts.DTOs.AiGeneration;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions.Content;

namespace SchoolJournal.Application.Common.Utils;

public static class CrosswordGenerator
{
    private const int MaxGridSize = 50;

    private sealed class InternalWord(string answer, string clue)
    {
        public int Number { get; set; }
        public string Answer { get; } = answer;
        public string Clue { get; } = clue;
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsHorizontal { get; set; }
    }

    private sealed class InternalCell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public char Letter { get; set; } = ' ';
        public bool IsEmpty { get; set; } = true;
        public string Number { get; set; } = string.Empty;
    }

    public static CrosswordContentDto Generate(IReadOnlyCollection<CrosswordAiWordDto> inputWords, int targetWordCount)
    {
        if (inputWords is null || inputWords.Count == 0) return new CrosswordContentDto(0, 0, [], []);

        var sorted = inputWords
            .Select(w => new InternalWord(w.Answer, w.Clue))
            .OrderByDescending(w => w.Answer.Length)
            .ToList();

        char[][] grid = InitializeGrid();
        var placedWords = new List<InternalWord>();

        PlaceFirstWord(grid, placedWords, sorted[0]);
        sorted.RemoveAt(0);

        PlaceRemainingWords(grid, placedWords, sorted, targetWordCount);

        return BuildFinalDto(placedWords);
    }

    private static char[][] InitializeGrid()
    {
        char[][] grid = new char[MaxGridSize][];
        for (int i = 0; i < MaxGridSize; i++)
        {
            grid[i] = new char[MaxGridSize];
            Array.Fill(grid[i], '\0'); // Використовуємо Null-символ замість пробілу для позначення порожньої клітинки сітки
        }
        return grid;
    }

    private static void PlaceFirstWord(char[][] grid, List<InternalWord> placedWords, InternalWord first)
    {
        first.Row = MaxGridSize / 2;
        first.Column = (MaxGridSize / 2) - (first.Answer.Length / 2);
        first.IsHorizontal = true;
        first.Number = 1;
        PlaceWord(grid, placedWords, first);
    }

    private static void PlaceRemainingWords(char[][] grid, List<InternalWord> placedWords, List<InternalWord> wordsToPlace, int targetWordCount)
    {
        int wordNumber = 2;

        foreach (var word in wordsToPlace)
        {
            // Жорсткий ліміт: як тільки розмістили потрібну вчителю кількість — зупиняємось, ігноруючи запас
            if (placedWords.Count >= targetWordCount) break;

            if (TryPlaceWord(grid, placedWords, word, wordNumber))
            {
                wordNumber++;
            }
        }
    }

    private static bool TryPlaceWord(char[][] grid, List<InternalWord> placedWords, InternalWord newWord, int wordNumber)
    {
        // Виправлення: Guid.NewGuid() безпечний замінник Random для простого перемішування колекцій
        foreach (var pWord in placedWords.OrderBy(_ => Guid.NewGuid()))
        {
            var intersections = GetIntersections(newWord.Answer, pWord.Answer);

            // Виправлення: Деконструкція змінних
            foreach (var (newIndex, placedIndex) in intersections.OrderBy(_ => Guid.NewGuid()))
            {
                // Виправлення Cognitive Complexity: логіка винесена в окремий метод
                if (TryPlaceAtIntersection(grid, placedWords, newWord, pWord, newIndex, placedIndex, wordNumber))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TryPlaceAtIntersection(
        char[][] grid,
        List<InternalWord> placedWords,
        InternalWord newWord,
        InternalWord pWord,
        int newIndex,
        int placedIndex,
        int wordNumber)
    {
        int startRow = pWord.IsHorizontal ? pWord.Row - newIndex : pWord.Row + placedIndex;
        int startCol = pWord.IsHorizontal ? pWord.Column + placedIndex : pWord.Column - newIndex;
        bool isHoriz = !pWord.IsHorizontal;

        if (CanPlace(grid, newWord.Answer, startRow, startCol, isHoriz))
        {
            newWord.Row = startRow;
            newWord.Column = startCol;
            newWord.IsHorizontal = isHoriz;
            newWord.Number = wordNumber;
            PlaceWord(grid, placedWords, newWord);
            return true;
        }
        return false;
    }

    private static CrosswordContentDto BuildFinalDto(List<InternalWord> placedWords)
    {
        if (placedWords.Count == 0) return new CrosswordContentDto(0, 0, [], []);

        int minR = placedWords.Min(w => w.Row);
        int minC = placedWords.Min(w => w.Column);
        int maxR = placedWords.Max(w => w.IsHorizontal ? w.Row : w.Row + w.Answer.Length - 1);
        int maxC = placedWords.Max(w => w.IsHorizontal ? w.Column + w.Answer.Length - 1 : w.Column);

        int width = maxC - minC + 1;
        int height = maxR - minR + 1;

        foreach (var w in placedWords)
        {
            w.Row -= minR;
            w.Column -= minC;
        }

        var cells = CreateInternalGrid(width, height);
        PopulateCellsWithWords(cells, placedWords, width);

        var finalWords = placedWords
            .OrderBy(w => w.Number)
            .Select(w => new CrosswordWordDto(w.Number, w.Answer, w.Clue, w.Row, w.Column, w.IsHorizontal))
            .ToList();

        var finalCells = cells
            .Select(c => new CrosswordCellDto(c.Row, c.Column, c.Letter, c.IsEmpty, c.Number))
            .ToList();

        return new CrosswordContentDto(width, height, finalWords, finalCells);
    }

    private static List<InternalCell> CreateInternalGrid(int width, int height)
    {
        var cells = new List<InternalCell>(width * height);
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                cells.Add(new InternalCell { Row = r, Column = c });
            }
        }
        return cells;
    }

    private static void PopulateCellsWithWords(List<InternalCell> cells, List<InternalWord> words, int width)
    {
        foreach (var word in words)
        {
            PopulateSingleWord(cells, word, width);
        }
    }

    private static void PopulateSingleWord(List<InternalCell> cells, InternalWord word, int width)
    {
        bool numberPlaced = false;
        for (int i = 0; i < word.Answer.Length; i++)
        {
            int r = word.IsHorizontal ? word.Row : word.Row + i;
            int c = word.IsHorizontal ? word.Column + i : word.Column;

            var cell = cells[r * width + c];
            cell.Letter = word.Answer[i];

            // Якщо це пробіл або дефіс — робимо клітинку візуально порожньою (пропуск)
            bool isGap = cell.Letter == ' ' || cell.Letter == '-';
            cell.IsEmpty = isGap;

            if (!numberPlaced && !isGap)
            {
                AddNumberToCell(cell, word.Number);
                numberPlaced = true;
            }
        }
    }

    private static void AddNumberToCell(InternalCell cell, int wordNumber)
    {
        string numStr = wordNumber.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(cell.Number))
        {
            cell.Number = numStr;
        }
        else if (!cell.Number.Contains(numStr, StringComparison.Ordinal))
        {
            cell.Number += $",{numStr}";
        }
    }

    private static void PlaceWord(char[][] grid, List<InternalWord> placedWords, InternalWord w)
    {
        placedWords.Add(w);
        for (int i = 0; i < w.Answer.Length; i++)
        {
            int r = w.IsHorizontal ? w.Row : w.Row + i;
            int c = w.IsHorizontal ? w.Column + i : w.Column;
            grid[r][c] = w.Answer[i]; // Виправлення синтаксису масиву
        }
    }

    private static bool CanPlace(char[][] grid, string word, int r, int c, bool isHorizontal)
    {
        if (!IsWithinBounds(word.Length, r, c, isHorizontal)) return false;
        if (!CanPlaceCharacters(grid, word, r, c, isHorizontal)) return false;
        if (HasEndAdjacencyConflict(grid, word.Length, r, c, isHorizontal)) return false;

        return true;
    }

    private static bool IsWithinBounds(int wordLength, int r, int c, bool isHorizontal)
    {
        if (r < 0 || c < 0) return false;
        int endR = isHorizontal ? r : r + wordLength - 1;
        int endC = isHorizontal ? c + wordLength - 1 : c;
        return endR < MaxGridSize && endC < MaxGridSize;
    }

    private static bool CanPlaceCharacters(char[][] grid, string word, int r, int c, bool isHorizontal)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int currR = isHorizontal ? r : r + i;
            int currC = isHorizontal ? c + i : c;

            if (grid[currR][currC] != '\0' && grid[currR][currC] != word[i]) return false;

            if (grid[currR][currC] == '\0' && HasSideAdjacencyConflict(grid, currR, currC, isHorizontal))
                return false;
        }
        return true;
    }

    private static bool HasSideAdjacencyConflict(char[][] grid, int currR, int currC, bool isHorizontal)
    {
        if (isHorizontal)
        {
            if (currR > 0 && grid[currR - 1][currC] != '\0') return true;
            if (currR < MaxGridSize - 1 && grid[currR + 1][currC] != '\0') return true;
        }
        else
        {
            if (currC > 0 && grid[currR][currC - 1] != '\0') return true;
            if (currC < MaxGridSize - 1 && grid[currR][currC + 1] != '\0') return true;
        }
        return false;
    }

    private static bool HasEndAdjacencyConflict(char[][] grid, int wordLength, int r, int c, bool isHorizontal)
    {
        if (isHorizontal)
        {
            bool hasLeftConflict = c > 0 && grid[r][c - 1] != '\0';
            bool hasRightConflict = c + wordLength < MaxGridSize && grid[r][c + wordLength] != '\0';
            return hasLeftConflict || hasRightConflict;
        }

        bool hasTopConflict = r > 0 && grid[r - 1][c] != '\0';
        bool hasBottomConflict = r + wordLength < MaxGridSize && grid[r + wordLength][c] != '\0';
        return hasTopConflict || hasBottomConflict;
    }

    private static List<(int NewIndex, int PlacedIndex)> GetIntersections(string newWord, string placedWord)
    {
        return [.. newWord
            .SelectMany((c1, i) => placedWord.Select((c2, j) => (c1, c2, NewIndex: i, PlacedIndex: j)))
            .Where(x => x.c1 == x.c2 && x.c1 != ' ' && x.c1 != '-') // Забороняємо кросворду перетинати слова на пробілах та дефісах!
            .Select(x => (x.NewIndex, x.PlacedIndex))];
    }
}