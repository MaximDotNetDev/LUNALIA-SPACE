namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface IClassroomRepository
{
    public Task<Guid> AddAsync(Classroom classroom, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByRoomNumberAsync(string roomNumber, CancellationToken cancellationToken = default);
    public Task<Classroom?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    public Task<Classroom?> UpdateAsync(Classroom classroom, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByRoomNumberExcludingIdAsync(string roomNumber, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Classroom?> DeleteAsync(Guid roomId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Classroom?> RestoreAsync(Guid roomId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Classroom> Items, int TotalCount)> GetActivePagedAsync(string? searchTerm, int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Classroom> Items, int TotalCount)> GetDeletedPagedAsync(string? searchTerm, int skip, int take, CancellationToken cancellationToken = default);
}