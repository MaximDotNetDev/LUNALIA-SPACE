using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Core.Student.CreateStudent;

public sealed class CreateStudentCommandHandler(
    IStudentRepository studentRepository,
    IAuditContext auditContext) : IRequestHandler<CreateStudentCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrEmpty(request.DocumentType) &&
            !string.IsNullOrEmpty(request.DocumentNumber) &&
            await studentRepository.ExistsByDocumentAsync(request.DocumentType, request.DocumentNumber, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict("Student.DuplicateDocument", "Учень з таким документом вже зареєстрований.");
        }

        var student = new Domain.Entities.Core.Student
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            DateOfBirth = request.DateOfBirth,
            ClassId = request.ClassId,
            Gender = Enum.TryParse<Gender>(request.Gender, true, out var g) ? g : Gender.None,
            DocumentType = request.DocumentType,
            DocumentSeries = request.DocumentSeries,
            DocumentNumber = request.DocumentNumber,
            EnrollmentDate = request.EnrollmentDate,
            EnrollmentReason = request.EnrollmentReason,
            Address = request.Address,
            MedicalNotes = request.MedicalNotes,
            UserId = request.UserId
        };

        var id = await studentRepository.AddAsync(student, cancellationToken).ConfigureAwait(false);

        var createdStudent = await studentRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (createdStudent is not null)
        {
            auditContext.TrackNewState(createdStudent);
        }

        return id;
    }
}