using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetStudentSubgroupById;

public sealed class GetStudentSubgroupByIdQueryHandler(
    IStudentSubgroupRepository repository)
    : IRequestHandler<GetStudentSubgroupByIdQuery, ErrorOr<StudentSubgroupResponse>>
{
    public async Task<ErrorOr<StudentSubgroupResponse>> Handle(GetStudentSubgroupByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var subgroup = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (subgroup is null || subgroup.IsDeleted)
        {
            return Error.NotFound(
                code: "StudentSubgroup.NotFound",
                description: "Призначення студента до підгрупи не знайдено.");
        }

        return subgroup.Adapt<StudentSubgroupResponse>();
    }
}