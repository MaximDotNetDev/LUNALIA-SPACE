using Dapper;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class TeacherRepository(SqlConnectionFactory connectionFactory) : ITeacherRepository
{
    public async Task<Guid> AddAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(teacher);

        const string sql = """
            INSERT INTO [Core].[Teachers] (
                LastName, FirstName, MiddleName, Phone, Specialization, 
                DateOfBirth, Gender, Workload, EducationInfo, MeetLink, 
                UserId, PositionId, QualificationId, PedagogicalTitleId, IsActive
            )
            OUTPUT INSERTED.TeacherId
            VALUES (
                @LastName, @FirstName, @MiddleName, @Phone, @Specialization, 
                @DateOfBirth, @Gender, @Workload, @EducationInfo, @MeetLink, 
                @UserId, @PositionId, @QualificationId, @PedagogicalTitleId, @IsActive
            );
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                teacher.LastName,
                teacher.FirstName,
                teacher.MiddleName,
                teacher.Phone,
                teacher.Specialization,
                teacher.DateOfBirth,
                Gender = teacher.Gender.ToString(),
                teacher.Workload,
                teacher.EducationInfo,
                teacher.MeetLink,
                teacher.UserId,
                teacher.PositionId,
                teacher.QualificationId,
                teacher.PedagogicalTitleId,
                teacher.IsActive
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Teacher?> GetByIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Teachers] WHERE TeacherId = @TeacherId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Teacher>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM [Core].[Teachers] WHERE Phone = @Phone AND IsDeleted = 0) THEN 1 ELSE 0 END AS BIT);";

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Phone = phone },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByPhoneExcludingIdAsync(string phone, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM [Core].[Teachers] WHERE Phone = @Phone AND TeacherId != @ExcludeId AND IsDeleted = 0) THEN 1 ELSE 0 END AS BIT);";
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { Phone = phone, ExcludeId = excludeId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Teacher?> UpdateProfileAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(teacher);

        const string sql = """
            UPDATE [Core].[Teachers]
            SET LastName = @LastName,
                FirstName = @FirstName,
                MiddleName = @MiddleName,
                Phone = @Phone,
                Specialization = @Specialization,
                DateOfBirth = @DateOfBirth,
                Gender = @Gender,
                EducationInfo = @EducationInfo,
                MeetLink = @MeetLink,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE TeacherId = @TeacherId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Teacher>(new CommandDefinition(
            sql,
            new
            {
                teacher.LastName,
                teacher.FirstName,
                teacher.MiddleName,
                teacher.Phone,
                teacher.Specialization,
                teacher.DateOfBirth,
                Gender = teacher.Gender.ToString(),
                teacher.EducationInfo,
                teacher.MeetLink,
                teacher.UpdatedAt,
                teacher.TeacherId,
                RowVersion = teacher.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Teacher?> UpdateAcademicInfoAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(teacher);

        const string sql = """
            UPDATE [Core].[Teachers]
            SET PositionId = @PositionId,
                QualificationId = @QualificationId,
                PedagogicalTitleId = @PedagogicalTitleId,
                Workload = @Workload,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE TeacherId = @TeacherId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Teacher>(new CommandDefinition(
            sql,
            new
            {
                teacher.PositionId,
                teacher.QualificationId,
                teacher.PedagogicalTitleId,
                teacher.Workload,
                teacher.UpdatedAt,
                teacher.TeacherId,
                RowVersion = teacher.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> IsUserAssignedToAnotherTeacherAsync(Guid userId, Guid excludeTeacherId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Teachers] 
                WHERE UserId = @UserId AND TeacherId != @ExcludeTeacherId AND IsDeleted = 0
            ) THEN 1 ELSE 0 END AS BIT);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { UserId = userId, ExcludeTeacherId = excludeTeacherId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Teacher?> AssignUserAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(teacher);

        const string sql = """
            UPDATE [Core].[Teachers]
            SET UserId = @UserId,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE TeacherId = @TeacherId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Teacher>(new CommandDefinition(
            sql,
            new
            {
                teacher.UserId,
                teacher.UpdatedAt,
                teacher.TeacherId,
                RowVersion = teacher.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Teacher?> ToggleStatusAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(teacher);

        const string sql = """
            UPDATE [Core].[Teachers]
            SET IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE TeacherId = @TeacherId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Teacher>(new CommandDefinition(
            sql,
            new
            {
                teacher.IsActive,
                teacher.UpdatedAt,
                teacher.TeacherId,
                RowVersion = teacher.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Teacher?> DeleteAsync(Guid teacherId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Teachers]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE TeacherId = @TeacherId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Teacher>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolJournal.Domain.Entities.Core.Models.TeacherDetailsResult?> GetDetailsByIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                t.TeacherId, t.LastName, t.FirstName, t.MiddleName, t.Phone, 
                t.Specialization, t.DateOfBirth, t.Gender, t.Workload, 
                t.EducationInfo, t.MeetLink, t.UserId, t.PositionId, 
                p.PositionName, t.QualificationId, q.QualificationName, 
                t.PedagogicalTitleId, pt.TitleName AS PedagogicalTitleName, 
                t.IsActive, t.IsDeleted, t.CreatedAt, t.UpdatedAt, t.RowVersion
            FROM [Core].[Teachers] t
            INNER JOIN [Reference].[Positions] p ON t.PositionId = p.PositionId
            INNER JOIN [Reference].[Qualifications] q ON t.QualificationId = q.QualificationId
            LEFT JOIN [Reference].[PedagogicalTitles] pt ON t.PedagogicalTitleId = pt.TitleId
            WHERE t.TeacherId = @TeacherId;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<SchoolJournal.Domain.Entities.Core.Models.TeacherDetailsResult>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<SchoolJournal.Domain.Entities.Core.Models.TeacherListItemResult> Items, int TotalCount)> GetPagedAsync(string? searchTerm, Guid? positionId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default)
    {
        var whereConditions = new System.Collections.Generic.List<string> { "t.IsDeleted = 0" };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereConditions.Add("(t.LastName LIKE '%' + @SearchTerm + '%' OR t.FirstName LIKE '%' + @SearchTerm + '%')");
        }

        if (positionId.HasValue)
        {
            whereConditions.Add("t.PositionId = @PositionId");
        }

        if (isActive.HasValue)
        {
            whereConditions.Add("t.IsActive = @IsActive");
        }

        var whereClause = "WHERE " + string.Join(" AND ", whereConditions);

        var sql = $"""
            SELECT COUNT(*) 
            FROM [Core].[Teachers] t
            {whereClause};

            SELECT 
                t.TeacherId, t.LastName, t.FirstName, t.MiddleName, t.Phone, 
                t.PositionId, p.PositionName, t.QualificationId, q.QualificationName, t.IsActive,
                t.UserId, u.Login
            FROM [Core].[Teachers] t
            INNER JOIN [Reference].[Positions] p ON t.PositionId = p.PositionId
            INNER JOIN [Reference].[Qualifications] q ON t.QualificationId = q.QualificationId
            LEFT JOIN [Identity].[Users] u ON t.UserId = u.UserId
            {whereClause}
            ORDER BY t.LastName, t.FirstName
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { SearchTerm = searchTerm, PositionId = positionId, IsActive = isActive, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<SchoolJournal.Domain.Entities.Core.Models.TeacherListItemResult>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<SchoolJournal.Domain.Entities.Core.Models.TeacherDetailsResult?> GetDetailsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                t.TeacherId, t.LastName, t.FirstName, t.MiddleName, t.Phone, 
                t.Specialization, t.DateOfBirth, t.Gender, t.Workload, 
                t.EducationInfo, t.MeetLink, t.UserId, t.PositionId, 
                p.PositionName, t.QualificationId, q.QualificationName, 
                t.PedagogicalTitleId, pt.TitleName AS PedagogicalTitleName, 
                t.IsActive, t.IsDeleted, t.CreatedAt, t.UpdatedAt, t.RowVersion
            FROM [Core].[Teachers] t
            INNER JOIN [Reference].[Positions] p ON t.PositionId = p.PositionId
            INNER JOIN [Reference].[Qualifications] q ON t.QualificationId = q.QualificationId
            LEFT JOIN [Reference].[PedagogicalTitles] pt ON t.PedagogicalTitleId = pt.TitleId
            WHERE t.UserId = @UserId AND t.IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<SchoolJournal.Domain.Entities.Core.Models.TeacherDetailsResult>(new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SchoolJournal.Domain.Entities.Core.Models.TeacherWorkloadResult>> GetWorkloadSummaryAsync(bool onlyActive, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                t.TeacherId, t.LastName, t.FirstName, t.MiddleName, 
                p.PositionName, ISNULL(t.Workload, 0) AS Workload, t.IsActive
            FROM [Core].[Teachers] t
            INNER JOIN [Reference].[Positions] p ON t.PositionId = p.PositionId
            WHERE t.IsDeleted = 0 
              AND (@OnlyActive = 0 OR t.IsActive = 1)
            ORDER BY t.Workload DESC, t.LastName ASC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<SchoolJournal.Domain.Entities.Core.Models.TeacherWorkloadResult>(new CommandDefinition(
            sql,
            new { OnlyActive = onlyActive },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}