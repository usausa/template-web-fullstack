namespace Template.WebApp.Services;

using Template.WebApp.Accessors;
using Template.WebApp.Models.Entity;

public sealed class DataService
{
    private readonly IDialect dialect;

    private readonly DataAccessor dataAccessor;

    private readonly TimeProvider timeProvider;

    public DataService(
        IDialect dialect,
        DataAccessor dataAccessor,
        TimeProvider timeProvider)
    {
        this.dialect = dialect;
        this.dataAccessor = dataAccessor;
        this.timeProvider = timeProvider;
    }

    public void CreateTable() =>
        dataAccessor.Create();

    public ValueTask<int> CountAsync(string? name) =>
        dataAccessor.CountAsync(name);

    public ValueTask<List<DataEntity>> QueryPageAsync(string? name, string? sort, bool desc, int offset, int size) =>
        dataAccessor.QueryPageAsync(name, sort, desc, offset, size);

    public ValueTask<List<DataEntity>> QueryAllAsync() =>
        dataAccessor.QueryAllAsync();

    public IAsyncEnumerable<DataEntity> QueryExportEnumerable(string? name, string? sort, bool desc, CancellationToken cancellationToken) =>
        dataAccessor.QueryExportEnumerable(name, sort, desc, cancellationToken);

    public ValueTask<DataEntity?> QueryAsync(long id) =>
        dataAccessor.QueryAsync(id);

    public async ValueTask<long?> InsertAsync(string name, int value)
    {
        try
        {
            return await dataAccessor.InsertAsync(name, value, timeProvider.GetLocalNow().DateTime);
        }
        catch (DbException ex)
        {
            if (dialect.IsDuplicate(ex))
            {
                return null;
            }

            throw;
        }
    }

    public async ValueTask<DataWriteStatus> UpdateAsync(long id, string name, int value)
    {
        try
        {
            var rows = await dataAccessor.UpdateAsync(id, name, value);
            return rows > 0 ? DataWriteStatus.Success : DataWriteStatus.NotFound;
        }
        catch (DbException ex)
        {
            if (dialect.IsDuplicate(ex))
            {
                return DataWriteStatus.Duplicate;
            }

            throw;
        }
    }

    public async ValueTask<bool> DeleteAsync(long id)
    {
        var rows = await dataAccessor.DeleteAsync(id);
        return rows > 0;
    }
}
