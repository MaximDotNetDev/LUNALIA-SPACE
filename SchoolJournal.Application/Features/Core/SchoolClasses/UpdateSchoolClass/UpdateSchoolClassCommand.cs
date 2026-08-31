using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.UpdateSchoolClass;

public sealed record UpdateSchoolClassCommand(
    Guid ClassId,
    string ClassName,
    int GradeLevel,
    string AcademicYear,
    Guid HomeroomTeacherId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;