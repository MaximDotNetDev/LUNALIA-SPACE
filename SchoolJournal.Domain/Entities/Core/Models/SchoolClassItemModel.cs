namespace SchoolJournal.Domain.Entities.Core.Models;

public sealed record SchoolClassItemModel
{
    public Guid ClassId { get; init; }
    public required string ClassName { get; init; }
    public int GradeLevel { get; init; }
    public required string AcademicYear { get; init; }
    public required string HomeroomTeacherLastName { get; init; }
    public required string HomeroomTeacherFirstName { get; init; }
    public string? HomeroomTeacherMiddleName { get; init; }
    public required string HomeroomTeacherFullName { get; init; }
}