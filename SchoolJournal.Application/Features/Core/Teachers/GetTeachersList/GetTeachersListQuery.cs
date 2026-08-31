using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Teachers;

namespace SchoolJournal.Application.Features.Core.Teachers.GetTeachersList;

public sealed record GetTeachersListQuery(
    PageRequest PageRequest,
    string? SearchTerm,
    Guid? PositionId,
    bool? IsActive
) : IRequest<ErrorOr<PagedResponse<TeacherListItemResponse>>>;