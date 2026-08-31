using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SchoolJournal.Desktop.Features.Operations.QuizExecution;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed partial class TakeQuizView : UserControl
{
    public TakeQuizView()
    {
        InitializeComponent();
    }

    // Зберігаємо поточний напрямок руху курсора учня (Пам'ять стану)
    private bool _isVerticalDirection;

    private void CrosswordCell_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { Text.Length: 1, DataContext: SchoolJournal.Client.Core.Features.Operations.QuizExecution.CrosswordCellItem currentCell } textBox &&
            FindParent<ItemsControl>(textBox) is { DataContext: SchoolJournal.Client.Core.Features.Operations.QuizExecution.QuestionExecutionItem quizItem } itemsControl)
        {
            SchoolJournal.Client.Core.Features.Operations.QuizExecution.CrosswordCellItem? nextCell = null;

            // Шукаємо наступну клітинку з урахуванням поточного напрямку (щоб не ламалося на перетинах)
            if (_isVerticalDirection)
            {
                nextCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row + 1 && c.Column == currentCell.Column && !c.IsEmpty)
                        ?? quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row && c.Column == currentCell.Column + 1 && !c.IsEmpty);
            }
            else
            {
                nextCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row && c.Column == currentCell.Column + 1 && !c.IsEmpty)
                        ?? quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row + 1 && c.Column == currentCell.Column && !c.IsEmpty);
            }

            if (nextCell != null)
            {
                // Динамічно оновлюємо напрямок на основі того, куди ми пішли (вниз чи вправо)
                _isVerticalDirection = nextCell.Row > currentCell.Row;

                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(nextCell) as FrameworkElement;
                var nextTextBox = FindVisualChild<TextBox>(container);

                nextTextBox?.Focus();
                nextTextBox?.SelectAll();
            }
        }
    }

    private void CrosswordCell_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is TextBox { DataContext: SchoolJournal.Client.Core.Features.Operations.QuizExecution.CrosswordCellItem currentCell } textBox &&
            FindParent<ItemsControl>(textBox) is { DataContext: SchoolJournal.Client.Core.Features.Operations.QuizExecution.QuestionExecutionItem quizItem } itemsControl)
        {
            SchoolJournal.Client.Core.Features.Operations.QuizExecution.CrosswordCellItem? targetCell = null;

            if (e.Key == System.Windows.Input.Key.Back)
            {
                textBox.Text = string.Empty; // Стираємо поточну літеру

                // Шукаємо попередню клітинку залежно від напрямку
                if (_isVerticalDirection)
                {
                    targetCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row - 1 && c.Column == currentCell.Column && !c.IsEmpty)
                              ?? quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row && c.Column == currentCell.Column - 1 && !c.IsEmpty);
                }
                else
                {
                    targetCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row && c.Column == currentCell.Column - 1 && !c.IsEmpty)
                              ?? quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row - 1 && c.Column == currentCell.Column && !c.IsEmpty);
                }

                if (targetCell != null)
                {
                    _isVerticalDirection = targetCell.Row < currentCell.Row;
                }
                e.Handled = true;
            }
            // Ручне керування курсором зі стрілочок на клавіатурі
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                _isVerticalDirection = false;
                targetCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row && c.Column == currentCell.Column + 1 && !c.IsEmpty);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Left)
            {
                _isVerticalDirection = false;
                targetCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row && c.Column == currentCell.Column - 1 && !c.IsEmpty);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                _isVerticalDirection = true;
                targetCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row + 1 && c.Column == currentCell.Column && !c.IsEmpty);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                _isVerticalDirection = true;
                targetCell = quizItem.CrosswordCells.FirstOrDefault(c => c.Row == currentCell.Row - 1 && c.Column == currentCell.Column && !c.IsEmpty);
                e.Handled = true;
            }

            if (targetCell != null)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(targetCell) as FrameworkElement;
                var targetTextBox = FindVisualChild<TextBox>(container);

                targetTextBox?.Focus();
                targetTextBox?.SelectAll();
            }
        }
    }

    // --- Логіка філворду (виділення протягуванням і кольорова палітра) ---
    private string _currentFillwordColor = "Transparent";
    private int _fillwordColorIndex;
    private SchoolJournal.Client.Core.Features.Operations.QuizExecution.FillwordCellItem? _lastFillwordCell;

    // Розширена палітра з 20 унікальних кольорів для виділення великої кількості слів
    private readonly string[] _fillwordPalette = [
        "#FF3B30", "#34C759", "#007AFF", "#AF52DE", "#FF9500",
        "#30B0C7", "#E22653", "#8D6E63", "#26A69A", "#D4A800",
        "#FF2D55", "#5856D6", "#5AC8FA", "#F0A500", "#4CD964",
        "#FF7043", "#7E57C2", "#EC407A", "#009688", "#607D8B"
    ];

    private void FillwordCell_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Border { DataContext: SchoolJournal.Client.Core.Features.Operations.QuizExecution.FillwordCellItem cell })
        {
            if (cell.IsSelected)
            {
                // Якщо клікнули на вже виділену літеру — скасовуємо її (Fail Fast)
                cell.IsSelected = false;
                cell.SelectionColor = "Transparent";
                _lastFillwordCell = null; // Розриваємо ланцюжок виділення
            }
            else
            {
                // Перевіряємо, чи є цей клік логічним продовженням попередньої літери (сусідня клітинка)
                bool isAdjacent = _lastFillwordCell != null && _lastFillwordCell.IsSelected &&
                                  (System.Math.Abs(_lastFillwordCell.Row - cell.Row) + System.Math.Abs(_lastFillwordCell.Column - cell.Column) == 1);

                if (!isAdjacent)
                {
                    // Початок нового слова — беремо наступний колір із палітри
                    _currentFillwordColor = _fillwordPalette[_fillwordColorIndex % _fillwordPalette.Length];
                    _fillwordColorIndex++;
                }

                cell.IsSelected = true;
                cell.SelectionColor = _currentFillwordColor;
                _lastFillwordCell = cell; // Запам'ятовуємо для наступних кліків
            }
        }
    }

    private void FillwordCell_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
            sender is Border { DataContext: SchoolJournal.Client.Core.Features.Operations.QuizExecution.FillwordCellItem { IsSelected: false } cell })
        {
            cell.IsSelected = true;
            cell.SelectionColor = _currentFillwordColor;
            _lastFillwordCell = cell;
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject? parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
    }

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild) return typedChild;
            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null) return childOfChild;
        }
        return null;
    }
}