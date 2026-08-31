using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentById;

public sealed class GetStudentByIdQueryHandler(IStudentRepository studentRepository)
    : IRequestHandler<GetStudentByIdQuery, ErrorOr<StudentResponse>>
{
    public async Task<ErrorOr<StudentResponse>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var student = await studentRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (student is null || student.IsDeleted)
        {
            return Error.NotFound("Student.NotFound", "Учня не знайдено.");
        }

        var response = student.Adapt<StudentResponse>() with
        {
            RowVersionBase64 = Convert.ToBase64String(student.RowVersion.ToArray())
        };

        return response;
    }
}