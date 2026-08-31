using Dapper;
using SchoolJournal.Domain.Entities.Infrastructure;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Infrastructure.Repositories;

public sealed class SystemSettingRepository(SqlConnectionFactory connectionFactory) : ISystemSettingRepository
{
    public async Task<SystemSetting?> GetAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Infrastructure].[SystemSettings] WHERE SettingKey = 1;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<SystemSetting>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SystemSetting?> UpsertAsync(SystemSetting setting, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setting);

        const string sql = """
            IF EXISTS (SELECT 1 FROM [Infrastructure].[SystemSettings] WHERE SettingKey = 1)
            BEGIN
                UPDATE [Infrastructure].[SystemSettings]
                SET SchoolName = @SchoolName,
                    AcademicYear = @AcademicYear,
                    PrincipalName = @PrincipalName,
                    UpdatedByUserId = @UpdatedByUserId,
                    UpdatedAt = @UpdatedAt
                OUTPUT INSERTED.*
                WHERE SettingKey = 1 AND RowVersion = @RowVersion;
            END
            ELSE
            BEGIN
                INSERT INTO [Infrastructure].[SystemSettings] 
                    (SettingKey, SchoolName, AcademicYear, PrincipalName, UpdatedByUserId, CreatedAt)
                OUTPUT INSERTED.*
                VALUES 
                    (1, @SchoolName, @AcademicYear, @PrincipalName, @UpdatedByUserId, @CreatedAt);
            END
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<SystemSetting>(new CommandDefinition(
            sql,
            new
            {
                setting.SchoolName,
                setting.AcademicYear,
                setting.PrincipalName,
                setting.UpdatedByUserId,
                setting.UpdatedAt,
                setting.CreatedAt,
                RowVersion = setting.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}