using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Operations.Quizzes;
using SchoolJournal.Client.Core.Features.Operations.QuizQuestions;
using SchoolJournal.Client.Core.Features.Operations.QuizExecution;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.Enums.Identity;
using SchoolJournal.Client.Core.Features.Operations.QuizAssignments; // Додано для StudentAssignmentsViewModel

namespace SchoolJournal.Client.Core.Features.Testing;

public sealed partial class TestingViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotTeacher))]
    public partial bool IsTeacher { get; set; }

    public bool IsNotTeacher => !IsTeacher;

    public TestingViewModel(IServiceProvider serviceProvider, IIdentityService identityService)
    {
        _serviceProvider = serviceProvider;
        _identityService = identityService;

        // Визначаємо, чи це вчитель/адмін
        IsTeacher = _identityService.IsInRole(RoleType.Admin, RoleType.Director, RoleType.Teacher);

        // Розумна навігація при відкритті модуля
        if (IsTeacher)
        {
            NavigateToQuizzes();
        }
        else
        {
            NavigateToStudentAssignments();
        }

        WeakReferenceMessenger.Default.Register<OpenQuizQuestionsMessage>(this, async (r, m) =>
        {
            var questionsVm = _serviceProvider.GetRequiredService<QuizQuestionsViewModel>();
            CurrentTestingView = questionsVm;
            // Змінюємо на ConfigureAwait(true) для стабільної роботи редактора питань викладача
            await questionsVm.InitializeAsync(m.QuizId).ConfigureAwait(true);
        });

        WeakReferenceMessenger.Default.Register<OpenAiGeneratorMessage>(this, async (r, m) =>
        {
            await NavigateToAiGeneratorAsync().ConfigureAwait(true);
        });

        WeakReferenceMessenger.Default.Register<OpenTakeQuizMessage>(this, async (r, m) =>
        {
            var takeQuizVm = _serviceProvider.GetRequiredService<TakeQuizViewModel>();
            CurrentTestingView = takeQuizVm;
            // Змінюємо на ConfigureAwait(true), щоб гарантовано повернутися в UI-потік для безпечного Data Binding
            await takeQuizVm.InitializeAsync(m.AssignmentId, m.QuizId).ConfigureAwait(true);
        });
    }

    [ObservableProperty]
    public partial ObservableObject? CurrentTestingView { get; set; }

    [RelayCommand]
    private void NavigateToQuizzes()
        => CurrentTestingView = _serviceProvider.GetRequiredService<QuizzesViewModel>();

    [RelayCommand]
    private void NavigateToQuizAssignments()
        => CurrentTestingView = _serviceProvider.GetRequiredService<QuizAssignmentsViewModel>();

    [RelayCommand]
    private async Task NavigateToAiGeneratorAsync()
    {
        var aiVm = _serviceProvider.GetRequiredService<AiQuizGeneratorViewModel>();
        CurrentTestingView = aiVm;
        await aiVm.InitializeAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private void NavigateToTakeQuiz()
        => CurrentTestingView = _serviceProvider.GetRequiredService<TakeQuizViewModel>();

    // НОВИЙ МЕТОД ДЛЯ УЧНЯ
    [RelayCommand]
    private void NavigateToStudentAssignments()
        => CurrentTestingView = _serviceProvider.GetRequiredService<StudentAssignmentsViewModel>();

    [RelayCommand]
    private void ToggleRoleMode()
    {
        // Перемикаємо режим (з Вчителя на Учня і навпаки)
        IsTeacher = !IsTeacher;

        // Автоматично відкриваємо потрібну вкладку після перемикання
        if (IsTeacher)
        {
            NavigateToQuizzes();
        }
        else
        {
            NavigateToStudentAssignments();
        }
    }
}

public sealed record OpenTakeQuizMessage(Guid AssignmentId, Guid QuizId);