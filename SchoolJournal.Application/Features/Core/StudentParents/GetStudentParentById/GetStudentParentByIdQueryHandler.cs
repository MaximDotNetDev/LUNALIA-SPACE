using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetStudentParentById;

public sealed class GetStudentParentByIdQueryHandler(
    IStudentParentRepository studentParentRepository)
    : IRequestHandler<GetStudentParentByIdQuery, ErrorOr<StudentParentResponse>>
{
    public async Task<ErrorOr<StudentParentResponse>> Handle(GetStudentParentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var studentParent = await studentParentRepository.GetByIdAsync(request.StudentParentId, cancellationToken).ConfigureAwait(false);

        if (studentParent is null)
        {
            return Error.NotFound(
                code: "StudentParent.NotFound",
                description: $"Зв'язок з ідентифікатором '{request.StudentParentId}' не знайдено.");
        }

        return new StudentParentResponse(
            studentParent.StudentParentId,
            studentParent.StudentId,
            studentParent.ParentId,
            studentParent.Role,
            studentParent.IsDeleted,
            studentParent.CreatedAt,
            studentParent.UpdatedAt);
    }
}