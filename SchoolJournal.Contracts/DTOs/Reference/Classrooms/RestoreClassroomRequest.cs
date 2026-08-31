namespace SchoolJournal.Contracts.DTOs.Reference.Classrooms;

public sealed record RestoreClassroomRequest(
    string RowVersionBase64
);