using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using SchoolJournal.Client.Core;
using SchoolJournal.Client.Core.Common.Auth;
using SchoolJournal.Desktop.Common.Auth;
using SchoolJournal.Desktop.Features.Academics;
using SchoolJournal.Desktop.Features.Administration;
using SchoolJournal.Desktop.Features.Communications;
using SchoolJournal.Desktop.Features.Core.Parents;
using SchoolJournal.Desktop.Features.Core.SchoolClasses;
using SchoolJournal.Desktop.Features.Core.StudentParents;
using SchoolJournal.Desktop.Features.Core.Students;
using SchoolJournal.Desktop.Features.Core.StudentSubgroups;
using SchoolJournal.Desktop.Features.Core.Subgroups;
using SchoolJournal.Desktop.Features.Core.Subject;
using SchoolJournal.Desktop.Features.Core.Teachers;
using SchoolJournal.Desktop.Features.Identity.Login;
using SchoolJournal.Desktop.Features.Infrastructure.Logs;
using SchoolJournal.Desktop.Features.Infrastructure.SystemSettings;
using SchoolJournal.Desktop.Features.Journal;
using SchoolJournal.Desktop.Features.Operations.TeachingAssignments;
using SchoolJournal.Desktop.Features.People;
using SchoolJournal.Desktop.Features.Reference.BellSchedule;
using SchoolJournal.Desktop.Features.Reference.Classroom;
using SchoolJournal.Desktop.Features.Reference.GradeType;
using SchoolJournal.Desktop.Features.Reference.LessonType;
using SchoolJournal.Desktop.Features.Reference.PedagogicalTitle;
using SchoolJournal.Desktop.Features.Reference.Position;
using SchoolJournal.Desktop.Features.Reference.Qualification;
using SchoolJournal.Desktop.Features.Reference.Semester;
using SchoolJournal.Desktop.Features.Schedule;
using SchoolJournal.Desktop.Features.Settings;
using SchoolJournal.Desktop.Features.Shell;
using SchoolJournal.Desktop.Features.Testing;
using SchoolJournal.Desktop.Features.Operations.FixedSchedules;
using SchoolJournal.Desktop.Features.Operations.Lessons;
using SchoolJournal.Desktop.Features.Operations.TeacherSubstitution;
using SchoolJournal.Desktop.Features.Operations.Grades;
using System.Windows;

namespace SchoolJournal.Desktop;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                string? apiUrl = context.Configuration["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(apiUrl))
                {
                    throw new InvalidOperationException("Налаштування 'ApiBaseUrl' відсутнє у файлі appsettings.json.");
                }

                // 1. Реєструємо клієнтське ядро (беремо URL з конфігурації)
                services.AddClientCore(new Uri(apiUrl));

                // Реєструємо конкретний клієнт (якщо його немає в AddClientCore)

                // 2. Реєструємо специфічне для Windows сховище токенів
                services.AddSingleton<ITokenStorageService, WindowsTokenStorageService>();

                // Реєструємо сервіс управління темами користувача
                services.AddSingleton<SchoolJournal.Client.Core.Common.Interfaces.IUserThemeService, SchoolJournal.Desktop.Common.Services.UserThemeService>();

                // 3. Реєструємо Вікна
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<AuditLogsView>();
                services.AddTransient<SettingsView>();

                services.AddTransient<SemestersView>();
                services.AddTransient<PositionsView>();
                services.AddTransient<QualificationsView>();
                services.AddTransient<SystemSettingsView>();
                services.AddTransient<PedagogicalTitlesView>();
                services.AddTransient<GradeTypesView>();
                services.AddTransient<LessonTypesView>();
                services.AddTransient<BellSchedulesView>();
                services.AddTransient<ClassroomsView>();

                services.AddTransient<JournalView>();
                services.AddTransient<AcademicsView>();
                services.AddTransient<PeopleView>();
                services.AddTransient<ScheduleView>();
                services.AddTransient<AdministrationView>();
                services.AddTransient<TestingView>();
                services.AddTransient<AnnouncementsView>();

                services.AddTransient<SubjectsView>();
                services.AddTransient<TeachersView>();
                services.AddTransient<ParentsView>();
                services.AddTransient<SchoolClassesView>();
                services.AddTransient<StudentsView>();
                services.AddTransient<SubgroupsView>();
                services.AddTransient<StudentParentsView>();
                services.AddTransient<StudentSubgroupsView>();

                services.AddTransient<TeachingAssignmentsView>();
                services.AddTransient<FixedSchedulesView>();
                services.AddTransient<LessonsView>();
                services.AddTransient<TeacherSubstitutionsView>();
                services.AddTransient<GradesView>();
                services.AddTransient<SchoolJournal.Desktop.Features.Operations.Attendances.LessonAttendanceRegisterView>();
                services.AddTransient<SchoolJournal.Desktop.Features.Operations.Quizzes.QuizzesView>();
                services.AddTransient<SchoolJournal.Desktop.Features.Operations.QuizQuestions.QuizQuestionsView>();
                services.AddTransient<SchoolJournal.Desktop.Features.Operations.QuizAssignments.QuizAssignmentsView>();
                services.AddTransient<SchoolJournal.Desktop.Features.Operations.Quizzes.AiQuizGeneratorView>();
                services.AddTransient<SchoolJournal.Desktop.Features.Operations.QuizExecution.TakeQuizView>();
                services.AddTransient<SchoolJournal.Desktop.Features.Operations.QuizAssignments.StudentAssignmentsView>();

            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await _host.StartAsync().ConfigureAwait(true);

        // Тимчасово забороняємо WPF автоматично закривати програму
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();

        // Показуємо вікно логіну як модальне
        if (loginWindow.ShowDialog() == true)
        {
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();

            // Робимо MainWindow новим головним вікном програми
            MainWindow = mainWindow;

            // Повертаємо стандартну поведінку: закрити програму при закритті MainWindow
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            mainWindow.Show();
        }
        else
        {
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync().ConfigureAwait(false);
        _host.Dispose();
        base.OnExit(e);
    }
}