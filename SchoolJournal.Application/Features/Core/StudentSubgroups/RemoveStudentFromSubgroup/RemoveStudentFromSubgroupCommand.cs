using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.RemoveStudentFromSubgroup;

public sealed record RemoveStudentFromSubgroupCommand(
    Guid StudentSubgroupId
) : IRequest<ErrorOr<Success>>;