using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Parents;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetParentsByStudentId;

public sealed class GetParentsByStudentIdQueryHandler(
    IStudentParentRepository studentParentRepository,
    IStudentRepository studentRepository)
    : IRequestHandler<GetParentsByStudentIdQuery, ErrorOr<IEnumerable<StudentParentDetailResponse>>>
{
    public async Task<ErrorOr<IEnumerable<StudentParentDetailResponse>>> Handle(GetParentsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var student = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
        if (student is null)
        {
            return Error.NotFound(
                code: "Student.NotFound",
                description: $"Учня з ідентифікатором '{request.StudentId}' не знайдено.");
        }

        var parentsData = await studentParentRepository.GetParentsByStudentIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);

        var response = parentsData.Select(d => new StudentParentDetailResponse(
            d.StudentParentId,
            d.Role,
            new ParentResponse(
                d.Parent.ParentId,
                d.Parent.LastName,
                d.Parent.FirstName,
                d.Parent.MiddleName,
                d.Parent.Phone,
                d.Parent.UserId,
                d.Parent.IsActive,
                d.Parent.CreatedAt,
                d.Parent.UpdatedAt,
                Convert.ToBase64String(d.Parent.RowVersion.ToArray()),
                null
            )
        ));

        return response.ToList();
    }
}