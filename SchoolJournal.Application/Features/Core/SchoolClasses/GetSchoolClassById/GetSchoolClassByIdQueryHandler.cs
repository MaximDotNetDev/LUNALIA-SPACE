using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.GetSchoolClassById;

public sealed class GetSchoolClassByIdQueryHandler(
    ISchoolClassRepository classRepository)
    : IRequestHandler<GetSchoolClassByIdQuery, ErrorOr<SchoolClassResponse>>
{
    public async Task<ErrorOr<SchoolClassResponse>> Handle(GetSchoolClassByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var classDetails = await classRepository.GetDetailsByIdAsync(request.ClassId, cancellationToken).ConfigureAwait(false);

        if (classDetails is null)
        {
            return Error.NotFound(
                code: "SchoolClass.NotFound",
                description: "Клас не знайдено або його було видалено.");
        }

        return classDetails.Adapt<SchoolClassResponse>();
    }
}