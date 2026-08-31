using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;

namespace SchoolJournal.Application.Features.Reference.Classroom.GetDeletedClassrooms;

public sealed record GetDeletedClassroomsQuery(
    PageRequest PageRequest,
    string? SearchTerm
) : IRequest<PagedResponse<ClassroomResponse>>;