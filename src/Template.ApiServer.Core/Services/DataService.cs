namespace Template.ApiServer.Services;

using Microsoft.Extensions.Caching.Hybrid;

using Template.ApiServer.Accessors;
using Template.ApiServer.Models;
using Template.ApiServer.Models.Entity;

public sealed class DataService
{
    // 一覧の結果はHybridCache(メモリ + Valkey)に載せ、更新系でタグごと無効化する。有効期限はHybridCacheの既定(L2は5分、L1は1分)
    private const string CacheTag = "data";

    private static readonly string[] CacheTags = [CacheTag];

    private readonly HybridCache cache;

    private readonly IDialect dialect;

    private readonly DataAccessor dataAccessor;

    private readonly ServiceContextProvider contextProvider;

    public DataService(
        HybridCache cache,
        IDialect dialect,
        DataAccessor dataAccessor,
        ServiceContextProvider contextProvider)
    {
        this.cache = cache;
        this.dialect = dialect;
        this.dataAccessor = dataAccessor;
        this.contextProvider = contextProvider;
    }

    public ValueTask<int> CountAsync(string? name, CancellationToken cancellationToken = default) =>
        dataAccessor.CountAsync(dialect.Match(name), cancellationToken);

    // 検索条件ごとにキャッシュする
    public ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, DataSort sort, bool desc, int page, int size, CancellationToken cancellationToken = default) =>
        cache.GetOrCreateAsync(
            $"data:list:{name}:{sort}:{desc}:{page}:{size}",
            ct => QueryPageCoreAsync(name, sort, desc, page, size, ct),
            tags: CacheTags,
            cancellationToken: cancellationToken);

    private async ValueTask<PagedResult<DataEntity>> QueryPageCoreAsync(string? name, DataSort sort, bool desc, int page, int size, CancellationToken cancellationToken)
    {
        var pattern = dialect.Match(name);
        var total = await dataAccessor.CountAsync(pattern, cancellationToken);
        var items = await dataAccessor.QueryPageAsync(pattern, sort, desc, size, page * size, cancellationToken);
        return new PagedResult<DataEntity>(total, page, size, items);
    }

    public ValueTask<DataEntity?> QueryAsync(long id) =>
        dataAccessor.QueryAsync(id);

    public async ValueTask<DataWriteStatus> InsertAsync(DataEntity entity)
    {
        var context = contextProvider.Current;

        try
        {
            entity.CreatedAt = context.Now.DateTime;
            entity.Version = 1;
            entity.Id = await dataAccessor.InsertAsync(entity.Name, entity.Value, entity.CreatedAt);
            await cache.RemoveByTagAsync(CacheTag);
            return DataWriteStatus.Success;
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return DataWriteStatus.Duplicate;
        }
    }

    // version を指定した更新は一致した行だけ書き換える(楽観的同時実行制御)。null なら無条件
    public async ValueTask<DataUpdateResult> UpdateAsync(long id, string name, int value, int? version = null)
    {
        try
        {
            var updated = await dataAccessor.UpdateAsync(id, name, value, version);
            if (updated.HasValue)
            {
                await cache.RemoveByTagAsync(CacheTag);
                return new DataUpdateResult(DataWriteStatus.Success, updated.Value);
            }

            // 行が無いのか版が違うのかを分ける
            var exists = version.HasValue && await dataAccessor.QueryAsync(id) is not null;
            return new DataUpdateResult(exists ? DataWriteStatus.VersionMismatch : DataWriteStatus.NotFound, 0);
        }
        catch (DbException ex) when (dialect.IsDuplicate(ex))
        {
            return new DataUpdateResult(DataWriteStatus.Duplicate, 0);
        }
    }

    public async ValueTask<DataWriteStatus> DeleteAsync(long id)
    {
        if (await dataAccessor.DeleteAsync(id) > 0)
        {
            await cache.RemoveByTagAsync(CacheTag);
            return DataWriteStatus.Success;
        }

        return DataWriteStatus.NotFound;
    }
}
