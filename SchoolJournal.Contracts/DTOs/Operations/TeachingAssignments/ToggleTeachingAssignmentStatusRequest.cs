namespace SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

public sealed record ToggleTeachingAssignmentStatusRequest(
    string RowVersionBase64
);