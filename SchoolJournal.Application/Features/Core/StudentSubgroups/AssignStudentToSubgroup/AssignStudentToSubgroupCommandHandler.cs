using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.AssignStudentToSubgroup;

public sealed class AssignStudentToSubgroupCommandHandler(
    IStudentSubgroupRepository repository,
    IAuditContext auditContext)
    : IRequestHandler<AssignStudentToSubgroupCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(AssignStudentToSubgroupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await repository.ExistsActiveAsync(request.StudentId, request.SubgroupId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "StudentSubgroup.AssignmentExists",
                description: "Студент вже призначений до цієї підгрупи.");
        }

        var studentSubgroup = new StudentSubgroup
        {
            StudentId = request.StudentId,
            SubgroupId = request.SubgroupId
        };

        var id = await repository.AddAsync(studentSubgroup, cancellationToken).ConfigureAwait(false);

        var newState = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return id;
    }
}