using Microsoft.Extensions.DependencyInjection;
using Refit;
using SchoolJournal.Client.Core.Common.Auth;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Client.Core.Features.Academics;
using SchoolJournal.Client.Core.Features.Administration;
using SchoolJournal.Client.Core.Features.Communications.Announcements;
using SchoolJournal.Client.Core.Features.Core.Parents;
using SchoolJournal.Client.Core.Features.Core.SchoolClasses;
using SchoolJournal.Client.Core.Features.Core.StudentParents;
using SchoolJournal.Client.Core.Features.Core.Students;
using SchoolJournal.Client.Core.Features.Core.StudentSubgroups;
using SchoolJournal.Client.Core.Features.Core.Subgroups;
using SchoolJournal.Client.Core.Features.Core.Subject;
using SchoolJournal.Client.Core.Features.Core.Teachers;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Client.Core.Features.Identity.Login;
using SchoolJournal.Client.Core.Features.Identity.Roles;
using SchoolJournal.Client.Core.Features.Infrastructure.Logs;
using SchoolJournal.Client.Core.Features.Infrastructure.Outbox;
using SchoolJournal.Client.Core.Features.Infrastructure.SystemSettings;
using SchoolJournal.Client.Core.Features.Journal;
using SchoolJournal.Client.Core.Features.Operations.FixedSchedules;
using SchoolJournal.Client.Core.Features.Operations.Grades;
using SchoolJournal.Client.Core.Features.Operations.Lessons;
using SchoolJournal.Client.Core.Features.Operations.QuizAssignments;
using SchoolJournal.Client.Core.Features.Operations.QuizExecution;
using SchoolJournal.Client.Core.Features.Operations.QuizSubmissions;
using SchoolJournal.Client.Core.Features.Operations.Quizzes;
using SchoolJournal.Client.Core.Features.Operations.TeacherSubstitution;
using SchoolJournal.Client.Core.Features.Operations.TeachingAssignments;
using SchoolJournal.Client.Core.Features.People;
using SchoolJournal.Client.Core.Features.Reference.BellSchedule;
using SchoolJournal.Client.Core.Features.Reference.Classroom;
using SchoolJournal.Client.Core.Features.Reference.GradeType;
using SchoolJournal.Client.Core.Features.Reference.LessonType;
using SchoolJournal.Client.Core.Features.Reference.PedagogicalTitle;
using SchoolJournal.Client.Core.Features.Reference.Position;
using SchoolJournal.Client.Core.Features.Reference.Qualification;
using SchoolJournal.Client.Core.Features.Reference.Semester;
using SchoolJournal.Client.Core.Features.Schedule;
using SchoolJournal.Client.Core.Features.Settings;
using SchoolJournal.Client.Core.Features.Shell;
using SchoolJournal.Client.Core.Features.Testing;
using System.Buffers.Text;

namespace SchoolJournal.Client.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientCore(this IServiceCollection services, Uri baseApiUrl)
    {
        ArgumentNullException.ThrowIfNull(baseApiUrl);

        services.AddTransient<AuthHeaderHandler>();

        services.AddRefitClient<IIdentityApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IAuditLogsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ISemesterApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ISystemSettingsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IPositionApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IQualificationApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IPedagogicalTitleApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IGradeTypeApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ILessonTypeApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IBellScheduleApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IClassroomApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IRoleApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IAnnouncementsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IOutboxApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ISubjectApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ITeacherApi>()
            .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IParentApi>()
            .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ISchoolClassApi>()
            .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IStudentApi>()
            .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ISubgroupsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IStudentParentApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IStudentSubgroupsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ITeachingAssignmentApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IFixedSchedulesApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ILessonApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<ITeacherSubstitutionApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<SchoolJournal.Client.Core.Features.Operations.Grades.IGradesApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<SchoolJournal.Client.Core.Features.Operations.Attendances.IAttendancesApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<SchoolJournal.Client.Core.Features.Operations.Quizzes.IQuizzesApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<SchoolJournal.Client.Core.Features.Operations.QuizQuestions.IQuizQuestionsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<SchoolJournal.Client.Core.Features.Operations.QuizAssignments.IQuizAssignmentsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<SchoolJournal.Client.Core.Features.Operations.QuizSubmissions.IQuizSubmissionsApi>()
                    .ConfigureHttpClient(client => client.BaseAddress = baseApiUrl)
                    .AddHttpMessageHandler<AuthHeaderHandler>();



        services.AddRefitClient<SchoolJournal.Client.Core.Features.Operations.Quizzes.IAiGenerationApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = baseApiUrl;
                                client.Timeout = TimeSpan.FromMinutes(10); // Даємо 10 хвилин на 100+ МБ файли
                            })
                            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<AuditLogsViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();

        services.AddTransient<SemestersViewModel>();
        services.AddTransient<PositionsViewModel>();
        services.AddTransient<SystemSettingsViewModel>();
        services.AddTransient<QualificationsViewModel>();
        services.AddTransient<PedagogicalTitlesViewModel>();
        services.AddTransient<GradeTypesViewModel>();
        services.AddTransient<LessonTypesViewModel>();
        services.AddTransient<BellSchedulesViewModel>();
        services.AddTransient<ClassroomsViewModel>();

        services.AddTransient<JournalViewModel>();
        services.AddTransient<AcademicsViewModel>();
        services.AddTransient<PeopleViewModel>();
        services.AddTransient<ScheduleViewModel>();
        services.AddTransient<AdministrationViewModel>();
        services.AddTransient<TestingViewModel>();
        services.AddTransient<AnnouncementsViewModel>();

        services.AddTransient<OutboxViewModel>();
        services.AddTransient<SubjectsViewModel>();
        services.AddTransient<TeachersViewModel>();
        services.AddTransient<ParentsViewModel>();
        services.AddTransient<SchoolClassesViewModel>();
        services.AddTransient<StudentsViewModel>();
        services.AddTransient<SubgroupsViewModel>();
        services.AddTransient<StudentParentsViewModel>();
        services.AddTransient<StudentSubgroupsViewModel>();

        services.AddTransient<TeachingAssignmentsViewModel>();
        services.AddTransient<FixedSchedulesViewModel>();
        services.AddTransient<LessonsViewModel>();
        services.AddTransient<TeacherSubstitutionsViewModel>();
        services.AddTransient<GradesViewModel>();
        services.AddTransient<SchoolJournal.Client.Core.Features.Operations.Attendances.LessonAttendanceRegisterViewModel>();
        services.AddTransient<SchoolJournal.Client.Core.Features.Operations.Quizzes.QuizzesViewModel>();
        services.AddTransient<SchoolJournal.Client.Core.Features.Operations.QuizQuestions.QuizQuestionsViewModel>();
        services.AddTransient<SchoolJournal.Client.Core.Features.Operations.QuizAssignments.QuizAssignmentsViewModel>();
        services.AddTransient<AiQuizGeneratorViewModel>();
        services.AddTransient<TakeQuizViewModel>();
        services.AddTransient<StudentAssignmentsViewModel>();


        return services;
    }
}