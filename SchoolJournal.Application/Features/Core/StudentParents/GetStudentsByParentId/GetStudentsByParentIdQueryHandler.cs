using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetStudentsByParentId;

public sealed class GetStudentsByParentIdQueryHandler(
    IStudentParentRepository studentParentRepository,
    IParentRepository parentRepository)
    : IRequestHandler<GetStudentsByParentIdQuery, ErrorOr<IEnumerable<ParentStudentDetailResponse>>>
{
    public async Task<ErrorOr<IEnumerable<ParentStudentDetailResponse>>> Handle(GetStudentsByParentIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parent = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);
        if (parent is null)
        {
            return Error.NotFound(
                code: "Parent.NotFound",
                description: $"Батьків з ідентифікатором '{request.ParentId}' не знайдено.");
        }

        var studentsData = await studentParentRepository.GetStudentsByParentIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);

        var response = studentsData.Select(d => new ParentStudentDetailResponse(
            d.StudentParentId,
            d.Role,
            new StudentResponse(
                d.Student.StudentId,
                d.Student.LastName,
                d.Student.FirstName,
                d.Student.MiddleName,
                d.Student.DateOfBirth,
                d.Student.ClassId,
                d.Student.Gender.ToString(),
                d.Student.DocumentType,
                d.Student.DocumentSeries,
                d.Student.DocumentNumber,
                d.Student.EnrollmentDate,
                d.Student.EnrollmentReason,
                d.Student.Address,
                d.Student.MedicalNotes,
                d.Student.UserId,
                d.Student.IsActive,
                Convert.ToBase64String(d.Student.RowVersion.ToArray())
            )
        ));

        return response.ToList();
    }
}