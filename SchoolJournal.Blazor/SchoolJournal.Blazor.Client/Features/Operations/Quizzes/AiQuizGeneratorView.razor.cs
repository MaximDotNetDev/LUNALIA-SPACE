using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SchoolJournal.Client.Core.Features.Operations.Quizzes;

namespace SchoolJournal.Blazor.Client.Features.Operations.Quizzes;

// Заглушаємо вимогу зробити клас internal, бо Blazor вимагає public для своїх компонентів
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Blazor components are generated as public partial classes")]
public sealed partial class AiQuizGeneratorView : IDisposable // Додали sealed, щоб вирішити CA1063
{
    [Parameter]
    public AiQuizGeneratorViewModel ViewModel { get; set; } = null!;

    private const long MaxFileSize = 200L * 1024 * 1024;

    protected override void OnInitialized()
    {
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "S5693", Justification = "Ліміт безпечний для клієнтського WASM додатку")]
    private void HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            if (file.Size > MaxFileSize)
            {
                ViewModel.ErrorMessage = $"Розмір файлу занадто великий. Максимум: {MaxFileSize / (1024 * 1024)} МБ.";
                ViewModel.SelectedFileName = null;
                ViewModel.FileStreamFactory = null;
                return;
            }

            ViewModel.ErrorMessage = string.Empty;
            ViewModel.SelectedFileName = file.Name;

            // Fail Fast: Передаємо фабрику замість потоку. ViewModel сама відкриє його "на льоту".
            ViewModel.FileStreamFactory = () => file.OpenReadStream(MaxFileSize);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            ViewModel.FileStreamFactory = null;
        }

        // Повідомляємо Garbage Collector, що ми коректно очистили ресурси (Вирішує CA1816)
        GC.SuppressFinalize(this);
    }
}