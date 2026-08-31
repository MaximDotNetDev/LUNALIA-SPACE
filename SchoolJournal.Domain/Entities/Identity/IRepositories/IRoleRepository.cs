namespace SchoolJournal.Domain.Entities.Identity.IRepositories;

public interface IRoleRepository
{
    public Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}