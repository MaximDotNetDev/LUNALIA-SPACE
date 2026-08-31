using Dapper;
using System.Data;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Infrastructure.Common.Persistence.Handlers;

internal sealed class RoleTypeHandler : SqlMapper.TypeHandler<RoleType>
{
    public override void SetValue(IDbDataParameter parameter, RoleType value)
        => parameter.Value = value.ToString();

    public override RoleType Parse(object value)
        => Enum.TryParse<RoleType>(value?.ToString(), true, out var result)
            ? result
            : RoleType.None;
}