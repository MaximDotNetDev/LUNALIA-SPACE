using Mapster;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;
using SchoolJournal.Domain.Entities.Core.Models;

namespace SchoolJournal.Application.Features.Core.SchoolClasses;

public sealed class SchoolClassMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.NewConfig<SchoolClassDetailsModel, SchoolClassResponse>()
            .Map(dest => dest.HomeroomTeacherFullName, src => FormatFullName(src.HomeroomTeacherLastName, src.HomeroomTeacherFirstName, src.HomeroomTeacherMiddleName))
            .Map(dest => dest.RowVersionBase64, src => Convert.ToBase64String(src.RowVersion.ToArray()));

        config.NewConfig<SchoolClassItemModel, SchoolClassItemResponse>()
            .Map(dest => dest.HomeroomTeacherFullName, src => FormatFullName(src.HomeroomTeacherLastName, src.HomeroomTeacherFirstName, src.HomeroomTeacherMiddleName));
    }

    private static string FormatFullName(string lastName, string firstName, string? middleName)
    {
        if (string.IsNullOrWhiteSpace(middleName))
        {
            return $"{lastName} {firstName}";
        }

        return $"{lastName} {firstName} {middleName}";
    }
}