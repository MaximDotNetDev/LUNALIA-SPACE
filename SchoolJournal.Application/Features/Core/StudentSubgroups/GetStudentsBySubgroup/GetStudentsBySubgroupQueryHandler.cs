using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetStudentsBySubgroup;

public sealed class GetStudentsBySubgroupQueryHandler(
    IStudentSubgroupRepository repository)
    : IRequestHandler<GetStudentsBySubgroupQuery, ErrorOr<SubgroupStudentsDetail>>
{
    public async Task<ErrorOr<SubgroupStudentsDetail>> Handle(GetStudentsBySubgroupQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var items = await repository.GetStudentsBySubgroupIdAsync(request.SubgroupId, cancellationToken).ConfigureAwait(false);

        var studentModels = items.Adapt<IEnumerable<SubgroupStudentModel>>();

        return new SubgroupStudentsDetail(
            SubgroupId: request.SubgroupId,
            Students: studentModels
        );
    }
}