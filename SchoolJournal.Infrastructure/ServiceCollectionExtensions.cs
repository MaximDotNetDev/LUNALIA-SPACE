using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;
using SchoolJournal.Application.Features.Operations.Lessons;
using SchoolJournal.Application.Features.Operations.TeachingAssignments;
using SchoolJournal.Domain.Entities.Communications.IRepositories;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Domain.Enums;
using SchoolJournal.Infrastructure.Common.Options;
using SchoolJournal.Infrastructure.Common.Persistence;
using SchoolJournal.Infrastructure.Common.Persistence.Handlers;
using SchoolJournal.Infrastructure.Modules.Communications.Repositories;
using SchoolJournal.Infrastructure.Modules.Core.Repositories;
using SchoolJournal.Infrastructure.Modules.Identity.Authentication;
using SchoolJournal.Infrastructure.Modules.Identity.Repositories;
using SchoolJournal.Infrastructure.Modules.Infrastructure.Repositories;
using SchoolJournal.Infrastructure.Modules.Operations.Queries;
using SchoolJournal.Infrastructure.Modules.Operations.Repositories;
using SchoolJournal.Infrastructure.Modules.Reference.Repositories;
using SchoolJournal.Infrastructure.Services.Ai;
using SchoolJournal.Infrastructure.Services.Files;


namespace SchoolJournal.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                    ?? throw new InvalidOperationException("Критична помилка: Секція конфігурації JWT відсутня або невірна.");
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();

        services.AddSingleton<SqlConnectionFactory>();
        SqlMapper.AddTypeHandler(new RoleTypeHandler());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IQualificationRepository, QualificationRepository>();
        services.AddScoped<IPedagogicalTitleRepository, PedagogicalTitleRepository>();
        services.AddScoped<IGradeTypeRepository, GradeTypeRepository>();
        services.AddScoped<ILessonTypeRepository, LessonTypeRepository>();
        services.AddScoped<IBellScheduleRepository, BellScheduleRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IParentRepository, ParentRepository>();
        services.AddScoped<ISchoolClassRepository, SchoolClassRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISubgroupRepository, SubgroupRepository>();
        services.AddScoped<IStudentParentRepository, StudentParentRepository>();
        services.AddScoped<IStudentSubgroupRepository, StudentSubgroupRepository>();
        services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
        services.AddScoped<ITeachingAssignmentQueries, TeachingAssignmentQueries>();
        services.AddScoped<IFixedScheduleRepository, FixedScheduleRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<ILessonQueries, LessonQueries>();
        services.AddScoped<ITeacherSubstitutionRepository, TeacherSubstitutionRepository>();  
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
        services.AddScoped<IQuizAssignmentRepository, QuizAssignmentRepository>();
        services.AddScoped<IQuizSubmissionRepository, QuizSubmissionRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();

        services.AddScoped<IPdfTextExtractionService, PdfTextExtractionService>();

        services.AddOptions<GeminiOptions>()
            .BindConfiguration(GeminiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IAiQuizGenerator, GeminiQuizGenerator>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        return services;
    }
}