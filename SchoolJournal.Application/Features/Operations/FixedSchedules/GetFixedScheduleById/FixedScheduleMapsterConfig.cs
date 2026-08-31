using Mapster;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.Models;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedScheduleById;

public sealed class FixedScheduleMapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.NewConfig<FixedScheduleReadModel, FixedScheduleResponse>()
                    .Map(dest => dest.DayOfWeek, src => (int)src.DayOfWeek)
                    .Map(dest => dest.RowVersionBase64, src => Convert.ToBase64String(src.RowVersion.ToArray()));
    }
}