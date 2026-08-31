using SchoolJournal.Domain.Entities.Operations.Models;

namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IAttendanceRepository
{
    public Task<Guid> AddAsync(Attendance attendance, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByLessonAndStudentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default);
    public Task<bool> LessonExistsAsync(Guid lessonId, CancellationToken cancellationToken = default);
    public Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken = default);
    public Task<bool> IsTeacherAssignedToLessonAsync(Guid teacherId, Guid lessonId, CancellationToken cancellationToken = default);
    public Task<Attendance?> GetByIdAsync(Guid attendanceId, CancellationToken cancellationToken = default);
    public Task<Attendance?> UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Attendance>> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default);
    public Task BulkUpsertAsync(Guid lessonId, IEnumerable<Attendance> attendances, CancellationToken cancellationToken = default);
    public Task<Attendance?> SoftDeleteAsync(Guid attendanceId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<bool> IsStudentUserAsync(Guid userId, Guid studentId, CancellationToken cancellationToken = default);
    public Task<bool> IsParentOfStudentUserAsync(Guid userId, Guid studentId, CancellationToken cancellationToken = default);
    public Task<LessonRegisterData?> GetLessonRegisterInternalAsync(Guid lessonId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<StudentAttendanceHistoryRow>> GetStudentHistoryInternalAsync(Guid studentId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default);
    public Task<bool> IsTeacherAssignedToStudentsClassAsync(Guid teacherId, Guid studentId, CancellationToken cancellationToken = default);
    public Task<StudentAttendanceStatsData> GetStudentStatsInternalAsync(Guid studentId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default);
}