using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.RestoreStudentInSubgroup;

public sealed record RestoreStudentInSubgroupCommand(
    Guid StudentSubgroupId
) : IRequest<ErrorOr<Success>>;