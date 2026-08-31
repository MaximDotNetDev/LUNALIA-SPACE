using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subjects;

namespace SchoolJournal.Application.Features.Core.Subject.GetDeletedSubjects;

public sealed record GetDeletedSubjectsQuery(
    PageRequest PageRequest,
    string? SearchTerm = null
) : IRequest<ErrorOr<PagedResponse<SubjectResponse>>>;