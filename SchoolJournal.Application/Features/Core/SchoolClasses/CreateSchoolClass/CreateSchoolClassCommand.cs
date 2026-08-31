using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.CreateSchoolClass;

public sealed record CreateSchoolClassCommand(
    string ClassName,
    int GradeLevel,
    string AcademicYear,
    Guid HomeroomTeacherId
) : IRequest<ErrorOr<Guid>>;