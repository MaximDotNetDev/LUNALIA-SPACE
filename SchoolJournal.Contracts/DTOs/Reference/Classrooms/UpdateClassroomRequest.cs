namespace SchoolJournal.Contracts.DTOs.Reference.Classrooms;

public sealed record UpdateClassroomRequest(
    string RoomNumber,
    string? Name,
    int Capacity,
    string RowVersionBase64
);