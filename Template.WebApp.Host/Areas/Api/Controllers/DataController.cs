namespace Template.WebApp.Host.Areas.Api.Controllers;

using Template.WebApp.Host.Areas.Api.Models;
using Template.WebApp.Host.Mappers;

public sealed class DataController : BaseApiController
{
    private DataService DataService { get; }

    private DataUsecase DataUsecase { get; }

    public DataController(
        DataService dataService,
        DataUsecase dataUsecase)
    {
        DataService = dataService;
        DataUsecase = dataUsecase;
    }

    //--------------------------------------------------------------------------------
    // Query
    //--------------------------------------------------------------------------------

    [HttpGet]
    [ProducesResponseType<DataListResponse>(StatusCodes.Status200OK)]
    public async ValueTask<IActionResult> List(
        [FromQuery] string? name,
        [FromQuery][Range(0, Int32.MaxValue)] int page = 0,
        [FromQuery][Range(1, 100)] int size = 20)
    {
        var result = await DataUsecase.QueryPageAsync(name, page, size);
        return Ok(new DataListResponse(result.Total, result.Page, result.Size, result.Items.Select(DataMapper.ToResponse).ToList()));
    }

    // ReSharper disable once RouteTemplates.RouteTokenNotResolved
    [HttpGet("~/[area]/[controller]/[action]/{id:long}")]
    [ProducesResponseType<DataResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Get(long id)
    {
        var entity = await DataService.QueryAsync(id);
        return entity is not null ? Ok(DataMapper.ToResponse(entity)) : NotFound();
    }

    //--------------------------------------------------------------------------------
    // Command
    //--------------------------------------------------------------------------------

    [HttpPost]
    [ProducesResponseType<DataCreateResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Create([FromBody] DataCreateRequest request)
    {
        var id = await DataService.InsertAsync(request.Name, request.Value);
        if (id is null)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Duplicate name.");
        }

        return CreatedAtAction(nameof(Get), new { id = id.Value }, new DataCreateResponse(id.Value));
    }

    // ReSharper disable once RouteTemplates.RouteTokenNotResolved
    [HttpPost("~/[area]/[controller]/[action]/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Update(long id, [FromBody] DataUpdateRequest request)
    {
        var result = await DataService.UpdateAsync(id, request.Name, request.Value);
        return result switch
        {
            DataWriteStatus.Success => NoContent(),
            DataWriteStatus.NotFound => NotFound(),
            _ => Problem(statusCode: StatusCodes.Status409Conflict, title: "Duplicate name.")
        };
    }

    // ReSharper disable once RouteTemplates.RouteTokenNotResolved
    [HttpPost("~/[area]/[controller]/[action]/{id:long}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete(long id)
    {
        return await DataService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
