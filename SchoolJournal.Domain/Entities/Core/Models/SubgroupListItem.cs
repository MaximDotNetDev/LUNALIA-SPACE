namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record SubgroupListItem
{
    public Guid SubgroupId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public string SubgroupName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}