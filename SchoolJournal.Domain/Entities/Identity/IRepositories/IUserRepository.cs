namespace SchoolJournal.Domain.Entities.Identity.IRepositories;

public interface IUserRepository
{
    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    public Task AddAsync(User user, CancellationToken cancellationToken = default);

    public Task UpdateCredentialsAsync(User user, CancellationToken cancellationToken = default);
}