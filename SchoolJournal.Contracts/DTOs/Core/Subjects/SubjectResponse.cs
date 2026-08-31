namespace SchoolJournal.Contracts.DTOs.Core.Subjects;

public sealed record SubjectResponse(
    Guid SubjectId,
    string SubjectName
);