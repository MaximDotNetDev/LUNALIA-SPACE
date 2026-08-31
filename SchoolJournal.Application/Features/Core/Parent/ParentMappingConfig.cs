using Mapster;
using SchoolJournal.Contracts.DTOs.Core.Parents;

namespace SchoolJournal.Application.Features.Core.Parent;

public sealed class ParentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.NewConfig<Domain.Entities.Core.Parent, ParentResponse>()
            .Map(dest => dest.RowVersionBase64, src => Convert.ToBase64String(src.RowVersion.ToArray()));
    }
}