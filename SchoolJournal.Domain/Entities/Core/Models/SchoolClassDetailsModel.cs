namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record SchoolClassDetailsModel
{
    public Guid ClassId { get; init; }
    public required string ClassName { get; init; }
    public int GradeLevel { get; init; }
    public required string AcademicYear { get; init; }
    public Guid HomeroomTeacherId { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];

    public required string HomeroomTeacherLastName { get; init; }
    public required string HomeroomTeacherFirstName { get; init; }
    public string? HomeroomTeacherMiddleName { get; init; }
}