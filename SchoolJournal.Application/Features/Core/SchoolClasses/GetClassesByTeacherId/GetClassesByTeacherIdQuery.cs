using MediatR;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.GetClassesByTeacherId;

public sealed record GetClassesByTeacherIdQuery(
    Guid TeacherId
) : IRequest<IEnumerable<SchoolClassItemResponse>>;