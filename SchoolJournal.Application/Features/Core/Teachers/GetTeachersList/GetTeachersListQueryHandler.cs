using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.GetTeachersList;

public sealed class GetTeachersListQueryHandler(ITeacherRepository teacherRepository)
    : IRequestHandler<GetTeachersListQuery, ErrorOr<PagedResponse<TeacherListItemResponse>>>
{
    public async Task<ErrorOr<PagedResponse<TeacherListItemResponse>>> Handle(GetTeachersListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await teacherRepository.GetPagedAsync(
            request.SearchTerm,
            request.PositionId,
            request.IsActive,
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var responseItems = items.Select(x => new TeacherListItemResponse(
                    x.TeacherId,
                    x.LastName,
                    x.FirstName,
                    x.MiddleName,
                    x.Phone,
                    x.PositionId,
                    x.PositionName,
                    x.QualificationId,
                    x.QualificationName,
                    x.IsActive,
                    x.UserId,   
                    x.Login    
                )).ToList();

        return new PagedResponse<TeacherListItemResponse>(
            responseItems,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}