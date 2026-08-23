namespace Template.WebApp.Usecase;

using Template.WebApp.Models;
using Template.WebApp.Models.Entity;
using Template.WebApp.Models.Paging;
using Template.WebApp.Services;

public sealed class DataUsecase
{
    private readonly DataService dataService;

    public DataUsecase(DataService dataService)
    {
        this.dataService = dataService;
    }

    public async ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, int page, int size)
    {
        var total = await dataService.CountAsync(name);
        var items = await dataService.QueryPageAsync(name, null, false, page * size, size);
        return new PagedResult<DataEntity>(total, page, size, items);
    }

    public async ValueTask<Paged<DataEntity>> QueryPagedAsync(string? name, string? sort, bool desc, Pageable pageable)
    {
        var count = await dataService.CountAsync(name);
        var items = await dataService.QueryPageAsync(name, sort, desc, pageable.Offset, pageable.Size);
        // ReSharper disable once UseCollectionExpression
        return new Paged<DataEntity>(pageable, items, count);
    }
}
