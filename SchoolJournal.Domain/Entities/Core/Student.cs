using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Domain.Entities.Core;

public sealed record Student
{
    public Guid StudentId { get; init; }

    public required string LastName { get; init; }

    public required string FirstName { get; init; }

    public string? MiddleName { get; init; }

    public DateTimeOffset? DateOfBirth { get; init; }

    public Guid ClassId { get; init; }

    public Gender Gender { get; init; }

    public string? DocumentType { get; init; }

    public string? DocumentSeries { get; init; }

    public string? DocumentNumber { get; init; }

    public DateTimeOffset? EnrollmentDate { get; init; }

    public string? EnrollmentReason { get; init; }

    public string? Address { get; init; }

    public string? MedicalNotes { get; init; }

    public Guid? UserId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}