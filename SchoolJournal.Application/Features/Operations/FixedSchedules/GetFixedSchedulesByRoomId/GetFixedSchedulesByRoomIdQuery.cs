using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByRoomId;

public sealed record GetFixedSchedulesByRoomIdQuery(
    Guid RoomId
) : IRequest<ErrorOr<IEnumerable<FixedScheduleResponse>>>;