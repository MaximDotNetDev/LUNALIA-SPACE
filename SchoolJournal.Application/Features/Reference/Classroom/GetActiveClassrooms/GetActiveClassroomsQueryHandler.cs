using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Classroom.GetActiveClassrooms;

public sealed class GetActiveClassroomsQueryHandler(IClassroomRepository classroomRepository)
    : IRequestHandler<GetActiveClassroomsQuery, PagedResponse<ClassroomResponse>>
{
    public async Task<PagedResponse<ClassroomResponse>> Handle(GetActiveClassroomsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await classroomRepository.GetActivePagedAsync(
            request.SearchTerm,
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var responses = items.Select(c => new ClassroomResponse(
            c.RoomId,
            c.RoomNumber,
            c.Name,
            c.Capacity,
            Convert.ToBase64String(c.RowVersion.ToArray())
        ));

        return new PagedResponse<ClassroomResponse>(
            responses,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}