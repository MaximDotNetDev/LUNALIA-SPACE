namespace SchoolJournal.Domain.Entities.Identity.IRepositories;

public interface IRefreshTokenRepository
{
    public Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    public Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);
}