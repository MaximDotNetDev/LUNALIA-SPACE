using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Classroom.GetClassroomById;

public sealed class GetClassroomByIdQueryHandler(IClassroomRepository classroomRepository)
    : IRequestHandler<GetClassroomByIdQuery, ErrorOr<ClassroomResponse>>
{
    public async Task<ErrorOr<ClassroomResponse>> Handle(GetClassroomByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var classroom = await classroomRepository.GetByIdAsync(request.RoomId, cancellationToken).ConfigureAwait(false);

        if (classroom is null)
        {
            return Error.NotFound(
                code: "Classroom.NotFound",
                description: "Аудиторію не знайдено.");
        }

        return new ClassroomResponse(
            classroom.RoomId,
            classroom.RoomNumber,
            classroom.Name,
            classroom.Capacity,
            Convert.ToBase64String(classroom.RowVersion.ToArray())
        );
    }
}