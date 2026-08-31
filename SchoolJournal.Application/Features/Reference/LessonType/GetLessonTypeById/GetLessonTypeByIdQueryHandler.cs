using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.LessonTypes;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.LessonType.GetLessonTypeById;

public sealed class GetLessonTypeByIdQueryHandler(ILessonTypeRepository lessonTypeRepository)
    : IRequestHandler<GetLessonTypeByIdQuery, ErrorOr<LessonTypeResponse>>
{
    public async Task<ErrorOr<LessonTypeResponse>> Handle(GetLessonTypeByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var lessonType = await lessonTypeRepository.GetByIdAsync(request.LessonTypeId, cancellationToken).ConfigureAwait(false);

        if (lessonType is null)
        {
            return Error.NotFound(
                code: "LessonType.NotFound",
                description: "Тип уроку не знайдено.");
        }

        return lessonType.Adapt<LessonTypeResponse>();
    }
}