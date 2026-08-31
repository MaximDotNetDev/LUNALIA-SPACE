using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Students;

namespace SchoolJournal.Application.Features.Core.Student.SearchStudents;

public sealed record SearchStudentsQuery(
    string? SearchTerm,
    Guid? ClassId,
    bool? IsActive,
    PageRequest Page) : IRequest<ErrorOr<PagedResponse<StudentSearchResponse>>>;