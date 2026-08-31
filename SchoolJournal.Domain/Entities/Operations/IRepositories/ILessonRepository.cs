namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface ILessonRepository
{
    public Task<Guid> AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
    public Task<Lesson?> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default);
    public Task<bool> VerifyAssignmentOwnershipAsync(Guid assignmentId, Guid userId, CancellationToken cancellationToken = default);
    public Task<bool> VerifyLessonOwnershipAsync(Guid lessonId, Guid userId, CancellationToken cancellationToken = default);
    public Task<Lesson?> UpdateTopicAndHomeworkAsync(Guid lessonId, string? lessonTopic, string? homework, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Lesson?> RescheduleAsync(Guid lessonId, DateTimeOffset lessonDate, Guid periodId, Guid roomId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Lesson?> DeleteAsync(Guid lessonId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Guid> GetSubjectIdByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);
}