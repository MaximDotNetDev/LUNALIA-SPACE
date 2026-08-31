namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record StudentSubgroupItem
{
    public Guid StudentSubgroupId { get; init; }
    public Guid SubgroupId { get; init; }
    public string SubgroupName { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public Guid SubjectId { get; init; }
}