using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;

namespace SchoolJournal.Application.Features.Reference.Semester.GetDeletedSemesters;

public sealed record GetDeletedSemestersQuery(PageRequest Pagination) : IRequest<PagedResponse<SemesterResponse>>;