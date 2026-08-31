using Dapper;
using SchoolJournal.Application.Features.Operations.TeachingAssignments;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Queries;

public sealed class TeachingAssignmentQueries(SqlConnectionFactory connectionFactory) : ITeachingAssignmentQueries
{
    private const string BaseSql = """
        SELECT 
            ta.AssignmentId, ta.TeacherId, 
            t.LastName + ' ' + t.FirstName AS TeacherFullName,
            ta.SubjectId, s.SubjectName,
            ta.ClassId, c.ClassName,
            ta.SubgroupId, sg.SubgroupName,
            ta.IsActive, ta.RowVersion
        FROM [Operations].[TeachingAssignments] ta
        INNER JOIN [Core].[Teachers] t ON ta.TeacherId = t.TeacherId
        INNER JOIN [Core].[Subjects] s ON ta.SubjectId = s.SubjectId
        INNER JOIN [Core].[Classes] c ON ta.ClassId = c.ClassId
        LEFT JOIN [Core].[Subgroups] sg ON ta.SubgroupId = sg.SubgroupId
        """;

    public async Task<(IEnumerable<TeachingAssignmentResponse> Items, int TotalCount)> GetPagedByTeacherIdAsync(Guid teacherId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = $"""
            SELECT COUNT(*) FROM [Operations].[TeachingAssignments] WHERE TeacherId = @TeacherId AND IsDeleted = 0;

            {BaseSql}
            WHERE ta.TeacherId = @TeacherId AND ta.IsDeleted = 0
            ORDER BY ta.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        return await ExecutePagedQueryAsync(sql, new { TeacherId = teacherId, Skip = skip, Take = take }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<TeachingAssignmentResponse> Items, int TotalCount)> GetPagedByClassIdAsync(Guid classId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = $"""
            SELECT COUNT(*) FROM [Operations].[TeachingAssignments] WHERE ClassId = @ClassId AND IsDeleted = 0;

            {BaseSql}
            WHERE ta.ClassId = @ClassId AND ta.IsDeleted = 0
            ORDER BY ta.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        return await ExecutePagedQueryAsync(sql, new { ClassId = classId, Skip = skip, Take = take }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<TeachingAssignmentResponse> Items, int TotalCount)> GetPagedBySubjectIdAsync(Guid subjectId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = $"""
            SELECT COUNT(*) FROM [Operations].[TeachingAssignments] WHERE SubjectId = @SubjectId AND IsDeleted = 0;

            {BaseSql}
            WHERE ta.SubjectId = @SubjectId AND ta.IsDeleted = 0
            ORDER BY ta.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        return await ExecutePagedQueryAsync(sql, new { SubjectId = subjectId, Skip = skip, Take = take }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(IEnumerable<TeachingAssignmentResponse> Items, int TotalCount)> ExecutePagedQueryAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var rawItems = await multi.ReadAsync<dynamic>().ConfigureAwait(false);

        var items = rawItems.Select(row => new TeachingAssignmentResponse(
            row.AssignmentId,
            row.TeacherId,
            row.TeacherFullName,
            row.SubjectId,
            row.SubjectName,
            row.ClassId,
            row.ClassName,
            row.SubgroupId,
            row.SubgroupName,
            row.IsActive,
            Convert.ToBase64String((byte[])row.RowVersion)
        ));

        return (items, totalCount);
    }

    public async Task<TeachingAssignmentResponse?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = $"""
            {BaseSql}
            WHERE ta.AssignmentId = @AssignmentId AND ta.IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();
        var rawItem = await connection.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
            sql, new { AssignmentId = assignmentId }, cancellationToken: cancellationToken)).ConfigureAwait(false);

        if (rawItem is null) return null;

        return new TeachingAssignmentResponse(
            rawItem.AssignmentId,
            rawItem.TeacherId,
            rawItem.TeacherFullName,
            rawItem.SubjectId,
            rawItem.SubjectName,
            rawItem.ClassId,
            rawItem.ClassName,
            rawItem.SubgroupId,
            rawItem.SubgroupName,
            rawItem.IsActive,
            Convert.ToBase64String((byte[])rawItem.RowVersion)
        );
    }
}