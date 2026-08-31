namespace SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

public interface ISystemSettingRepository
{
    public Task<SystemSetting?> GetAsync(CancellationToken cancellationToken = default);
    public Task<SystemSetting?> UpsertAsync(SystemSetting setting, CancellationToken cancellationToken = default);
}