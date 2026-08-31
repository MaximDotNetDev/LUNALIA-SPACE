namespace SchoolJournal.Contracts.DTOs.Reference.Classrooms;

public sealed record CreateClassroomRequest(
    string RoomNumber,
    string? Name,
    int Capacity
);