namespace Template.WebApp.Accessors;

[DataAccessor]
public sealed partial class DataAccessor
{
    [Execute]
    public partial void Create();

    [ExecuteScalar]
    public partial ValueTask<int> CountAsync(string? name);

    [Query]
    public partial ValueTask<List<DataEntity>> QueryPageAsync(string? name, string? sort, bool desc, int offset, int size);

    [Query]
    public partial ValueTask<List<DataEntity>> QueryAllAsync();

    // ReSharper disable once CSharpWarnings::CS8424 (SDA0305がEnumeratorCancellation注釈を要求するため。実装はジェネレータ側で処理される)
    [Query]
    public partial IAsyncEnumerable<DataEntity> QueryExportEnumerable(string? name, string? sort, bool desc, [EnumeratorCancellation] CancellationToken cancellationToken);

    [QueryFirst]
    public partial ValueTask<DataEntity?> QueryAsync(long id);

    [ExecuteScalar]
    public partial ValueTask<long> InsertAsync(string name, int value, DateTime createdAt);

    [Execute]
    public partial ValueTask<int> UpdateAsync(long id, string name, int value);

    [Execute]
    public partial ValueTask<int> DeleteAsync(long id);
}
