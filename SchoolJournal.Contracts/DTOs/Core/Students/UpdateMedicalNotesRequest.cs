namespace SchoolJournal.Contracts.DTOs.Core.Students;

public sealed record UpdateMedicalNotesRequest(
    string? MedicalNotes,
    string RowVersionBase64);