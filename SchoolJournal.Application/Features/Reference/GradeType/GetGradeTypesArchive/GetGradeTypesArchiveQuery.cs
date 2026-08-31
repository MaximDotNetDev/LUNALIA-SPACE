using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;

namespace SchoolJournal.Application.Features.Reference.GradeType.GetGradeTypesArchive;

public sealed record GetGradeTypesArchiveQuery(PageRequest PageRequest) : IRequest<PagedResponse<GradeTypeResponse>>;