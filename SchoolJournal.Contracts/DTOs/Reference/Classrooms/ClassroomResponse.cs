namespace SchoolJournal.Contracts.DTOs.Reference.Classrooms;

public sealed record ClassroomResponse(
    Guid RoomId,
    string RoomNumber,
    string? Name,
    int Capacity,
    string RowVersionBase64
);