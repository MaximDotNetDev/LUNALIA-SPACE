using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SchoolJournal.Client.Core.Features.Operations.Quizzes;

namespace SchoolJournal.Desktop.Features.Operations.Quizzes;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed partial class AiQuizGeneratorView : UserControl
{
    public AiQuizGeneratorView()
    {
        InitializeComponent();
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            Title = "Оберіть навчальний матеріал у форматі PDF"
        };

        if (dialog.ShowDialog() == true && DataContext is AiQuizGeneratorViewModel vm)
        {
            vm.SelectedFileName = System.IO.Path.GetFileName(dialog.FileName);

            // Передаємо делегат. ViewModel сама відкриє новий потік у потрібний момент.
            vm.FileStreamFactory = () => System.IO.File.OpenRead(dialog.FileName);
        }
    }
}