namespace Template.ApiServer.Services;

using Microsoft.Extensions.Caching.Hybrid;

using Template.ApiServer.Accessors;
using Template.ApiServer.Infrastructure.Data;
using Template.ApiServer.Models;
using Template.ApiServer.Models.Entity;

public sealed class DataService
{
    // 並べ替えに使える列。SqlHelper.NormalizeSortがこの集合以外を弾く
    private static readonly string[] SortKeys = ["Name", "Value", "CreatedAt"];

    // 一致しなかったときの並び順。テーブルの主キー
    private const string DefaultSortColumn = "Id";

    // 一覧の結果はHybridCache(メモリ + Valkey)に載せ、更新系でタグごと無効化する。有効期限はHybridCacheの既定(L2は5分、L1は1分)
    private const string CacheTag = "data";

    private static readonly string[] CacheTags = [CacheTag];

    private readonly TimeProvider timeProvider;

    private readonly HybridCache cache;

    private readonly IDialect dialect;

    private readonly DataAccessor dataAccessor;

    public DataService(
        TimeProvider timeProvider,
        HybridCache cache,
        IDialect dialect,
        DataAccessor dataAccessor)
    {
        this.timeProvider = timeProvider;
        this.cache = cache;
        this.dialect = dialect;
        this.dataAccessor = dataAccessor;
    }

    public void CreateTable() =>
        dataAccessor.Create();

    public ValueTask<int> CountAsync(string? name, CancellationToken cancellationToken = default) =>
        dataAccessor.CountAsync(name, cancellationToken);

    // ページ番号と件数で扱い、総件数と合わせて返す。検索条件ごとにキャッシュする
    public ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, string? sort, bool desc, int page, int size, CancellationToken cancellationToken = default) =>
        cache.GetOrCreateAsync(
            $"data:list:{name}:{sort}:{desc}:{page}:{size}",
            ct => QueryPageCoreAsync(name, sort, desc, page, size, ct),
            tags: CacheTags,
            cancellationToken: cancellationToken);

    private async ValueTask<PagedResult<DataEntity>> QueryPageCoreAsync(string? name, string? sort, bool desc, int page, int size, CancellationToken cancellationToken)
    {
        var total = await dataAccessor.CountAsync(name, cancellationToken);
        var items = await dataAccessor.QueryPageAsync(name, SqlHelper.NormalizeSort(SortKeys, DefaultSortColumn, sort, desc), page * size, size, cancellationToken);
        return new PagedResult<DataEntity>(total, page, size, items);
    }

    public ValueTask<DataEntity?> QueryAsync(long id) =>
        dataAccessor.QueryAsync(id);

    public async ValueTask<long?> InsertAsync(string name, int value)
    {
        try
        {
            var id = await dataAccessor.InsertAsync(name, value, timeProvider.GetLocalNow().DateTime);
            await cache.RemoveByTagAsync(CacheTag);
            return id;
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
            if (rows > 0)
            {
                await cache.RemoveByTagAsync(CacheTag);
                return DataWriteStatus.Success;
            }

            return DataWriteStatus.NotFound;
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
        if (rows > 0)
        {
            await cache.RemoveByTagAsync(CacheTag);
            return true;
        }

        return false;
    }
}
