using SchoolJournal.Domain.Entities.Core.Models;

namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface IStudentRepository
{
    public Task<Guid> AddAsync(Student student, CancellationToken cancellationToken = default);
    public Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByDocumentAsync(string type, string number, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByDocumentExcludingIdAsync(string type, string number, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Student?> UpdateAsync(Student student, CancellationToken cancellationToken = default);
    public Task<Student?> DeleteAsync(Guid studentId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Student?> TransferToClassAsync(Guid studentId, Guid newClassId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<bool> IsUserAlreadyLinkedAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<Student?> LinkUserAsync(Guid studentId, Guid userId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Student?> UpdateMedicalNotesAsync(Guid studentId, string? medicalNotes, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Student>> GetActiveByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<StudentSearchResult> Items, int TotalCount)> SearchAsync(
            string? searchTerm,
            Guid? classId,
            bool? isActive,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
    public Task<IEnumerable<StudentHistory>> GetHistoryAsync(Guid studentId, CancellationToken cancellationToken = default);
    public Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}