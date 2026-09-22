namespace Template.ApiServer.Accessors;

using Template.ApiServer.Models.Entity;

[DataAccessor]
public sealed partial class DataAccessor
{
    [Execute]
    public partial void Create();

    // PostgreSQLはnullの型を推定できない(42P18)ので型を明示する
    [ExecuteScalar]
    public partial ValueTask<int> CountAsync([DbType(DbType.String)] string? name, CancellationToken cancellationToken);

    [Query]
    public partial ValueTask<List<DataEntity>> QueryPageAsync([DbType(DbType.String)] string? name, string order, int offset, int size, CancellationToken cancellationToken);

    [QueryFirst]
    public partial ValueTask<DataEntity?> QueryAsync(long id);

    [ExecuteScalar]
    public partial ValueTask<long> InsertAsync(string name, int value, DateTime createdAt);

    [ExecuteScalar]
    public partial ValueTask<int?> UpdateAsync(long id, string name, int value, [DbType(DbType.Int32)] int? version);

    [Execute]
    public partial ValueTask<int> DeleteAsync(long id);
}
