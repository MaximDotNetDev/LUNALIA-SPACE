using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.TransferStudentToAnotherSubgroup;

public sealed record TransferStudentToAnotherSubgroupCommand(
    Guid StudentSubgroupId,
    Guid NewSubgroupId
) : IRequest<ErrorOr<Success>>;