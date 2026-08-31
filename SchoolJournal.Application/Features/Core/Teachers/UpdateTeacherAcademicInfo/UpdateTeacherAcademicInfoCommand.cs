using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherAcademicInfo;

public sealed record UpdateTeacherAcademicInfoCommand(
    Guid TeacherId,
    Guid PositionId,
    Guid QualificationId,
    Guid? PedagogicalTitleId,
    decimal? Workload,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;