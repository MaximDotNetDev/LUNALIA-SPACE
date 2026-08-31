using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.GetClassesByTeacherId;

public sealed class GetClassesByTeacherIdQueryHandler(
    ISchoolClassRepository classRepository)
    : IRequestHandler<GetClassesByTeacherIdQuery, IEnumerable<SchoolClassItemResponse>>
{
    public async Task<IEnumerable<SchoolClassItemResponse>> Handle(GetClassesByTeacherIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var items = await classRepository.GetByTeacherIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);

        return items.Adapt<IEnumerable<SchoolClassItemResponse>>();
    }
}