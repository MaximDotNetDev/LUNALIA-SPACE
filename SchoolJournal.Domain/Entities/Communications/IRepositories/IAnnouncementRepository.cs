namespace SchoolJournal.Domain.Entities.Communications.IRepositories;

public interface IAnnouncementRepository
{
    public Task<Guid> AddAsync(Announcement announcement, CancellationToken cancellationToken = default);

    public Task<Announcement?> GetByIdAsync(Guid announcementId, CancellationToken cancellationToken = default);

    public Task<Announcement?> UpdateAsync(Announcement announcement, CancellationToken cancellationToken = default);

    public Task<Announcement?> ToggleStatusAsync(Guid announcementId, byte[] rowVersion, CancellationToken cancellationToken = default);

    public Task<Announcement?> DeleteAsync(Guid announcementId, byte[] rowVersion, CancellationToken cancellationToken = default);

    public Task<(IEnumerable<Announcement> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default);

    public Task<(IEnumerable<Announcement> Items, int TotalCount)> GetAdminPagedAsync(int skip, int take, string? searchTerm, bool? isActive, Guid? authorId, CancellationToken cancellationToken = default);
}