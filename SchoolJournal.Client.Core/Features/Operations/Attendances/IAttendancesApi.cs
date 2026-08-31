using Refit;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolJournal.Client.Core.Features.Operations.Attendances;

public interface IAttendancesApi
{
    [Post("/api/attendances")]
    public Task<IApiResponse<object>> RecordAttendanceAsync([Body] RecordAttendanceRequest request, CancellationToken ct = default);

    [Put("/api/attendances/{id}")]
    public Task<IApiResponse> UpdateAttendanceAsync(Guid id, [Body] UpdateAttendanceRequest request, CancellationToken ct = default);

    [Post("/api/attendances/bulk")]
    public Task<IApiResponse> BulkRecordAttendanceAsync([Body] BulkRecordAttendanceRequest request, CancellationToken ct = default);

    [Delete("/api/attendances/{id}")]
    public Task<IApiResponse> DeleteAttendanceAsync(Guid id, [Body] DeleteAttendanceRequest request, CancellationToken ct = default);

    [Get("/api/attendances/{id}")]
    public Task<IApiResponse<AttendanceResponse>> GetAttendanceByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/attendances/lessons/{lessonId}/register")]
    public Task<IApiResponse<LessonAttendanceRegisterResponse>> GetLessonAttendanceRegisterAsync(Guid lessonId, CancellationToken ct = default);

    [Get("/api/attendances/students/{studentId}/history")]
    public Task<IApiResponse<StudentAttendanceHistoryResponse>> GetStudentAttendanceHistoryAsync(Guid studentId, [Query] DateTimeOffset? startDate, [Query] DateTimeOffset? endDate, CancellationToken ct = default);

    [Get("/api/attendances/students/{studentId}/stats")]
    public Task<IApiResponse<StudentAttendanceStatsResponse>> GetStudentAttendanceStatsAsync(Guid studentId, [Query] DateTimeOffset? startDate, [Query] DateTimeOffset? endDate, CancellationToken ct = default);
}