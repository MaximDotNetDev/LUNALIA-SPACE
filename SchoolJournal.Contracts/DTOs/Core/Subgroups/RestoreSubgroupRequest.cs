namespace SchoolJournal.Contracts.DTOs.Core.Subgroups;

public sealed record RestoreSubgroupRequest(
    string RowVersionBase64
);