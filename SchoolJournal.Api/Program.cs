using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using SchoolJournal.Api.Common.Exceptions;
using SchoolJournal.Api.Endpoints.Ai;
using SchoolJournal.Api.Endpoints.Communications;
using SchoolJournal.Api.Endpoints.Core;
using SchoolJournal.Api.Endpoints.Identity;
using SchoolJournal.Api.Endpoints.Infrastructure;
using SchoolJournal.Api.Endpoints.Operations;
using SchoolJournal.Api.Endpoints.Reference;
using SchoolJournal.Application;
using SchoolJournal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();

// НАЛАШТУВАННЯ OPENAPI: Додаємо сервери, щоб Swagger знав, куди штурхати запити
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers?.Clear(); // Очищаємо автозгенеровані сервери, щоб уникнути дублів
        document.Servers?.Add(new Microsoft.OpenApi.OpenApiServer
        {
            Url = "https://lunalia-space.onrender.com" // Оновлена адреса
        });
        return Task.CompletedTask;
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login_limiter", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
            new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 268435456; // 256 МБ у байтах
    options.MemoryBufferThreshold = int.MaxValue;
});

// 2. Розширюємо ліміт самого сервера Kestrel до 256 МБ
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 268435456; // 256 МБ у байтах
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasmPolicy", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7223",
                "https://192.168.137.1:7223",
                "https://192.168.1.104:7223",
                "https://lunalia-blazor.onrender.com"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler();

//if (app.Environment.IsDevelopment())
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SchoolJournal API v1");
        options.RoutePrefix = "swagger";
    });


// Активуємо CORS політику перед авторизацією!
app.UseCors("BlazorWasmPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapLogin();
app.MapRefresh();
app.MapLogoutEndpoint();

app.MapAuditLogs();
app.MapSemesters();
app.MapQualifications();
app.MapPositions();
app.MapPedagogicalTitles();
app.MapGradeTypes();
app.MapSystemSettings();
app.MapLessonTypes();
app.MapBellSchedules();
app.MapClassrooms();
app.MapRoles();
app.MapAnnouncements();
app.MapOutboxMessages();
app.MapSubjects();
app.MapTeachers();
app.MapParents();
app.MapSchoolClasses();
app.MapStudents();
app.MapSubgroups();
app.MapStudentParents();
app.MapStudentSubgroups();
app.MapTeachingAssignments();
app.MapFixedSchedules();
app.MapLessons();
app.MapTeacherSubstitutions();
app.MapGrades();
app.MapAttendances();
app.MapQuizzes();
app.MapQuizQuestions();
app.MapQuizAssignments();
app.MapAiGeneration();
app.MapRegister();
app.MapUpdateAccount();
app.MapQuizSubmissions();

await app.RunAsync().ConfigureAwait(false);
