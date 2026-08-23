namespace Template.WebApp.Host.Areas.Default.Controllers;

using CsvHelper;

using Smart.AspNetCore.Mvc;

using Template.WebApp.Host.Areas.Default.Models;
using Template.WebApp.Host.Infrastructure.Reports;
using Template.WebApp.Host.Mappers;

public sealed class DataController : BaseDefaultController
{
    private const int PageSize = 15;

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
    // List
    //--------------------------------------------------------------------------------

    [HttpGet]
    public async ValueTask<IActionResult> List([FromQuery] DataListCondition c)
    {
        if (!Request.IsInitialRequest() && c.Go && ModelState.IsValid)
        {
            c.Page = Math.Max(c.Page, 1);

            var paged = await DataUsecase.QueryPagedAsync(c.Name, c.Sort, c.Desc, c.SetSize(PageSize));
            if (paged.IsOver)
            {
                return RedirectToAction(nameof(List), new { c.Go, c.Name, c.Sort, c.Desc, Page = paged.TotalPage });
            }

            ViewBag.Paged = paged;
        }

        return View(c);
    }

    //--------------------------------------------------------------------------------
    // Details
    //--------------------------------------------------------------------------------

    [HttpGet("~/[controller]/[action]/{id:long}")]
    public async ValueTask<IActionResult> Details(long id)
    {
        var entity = await DataService.QueryAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        return View(entity);
    }

    //--------------------------------------------------------------------------------
    // Create
    //--------------------------------------------------------------------------------

    [HttpGet]
    public IActionResult Create()
    {
        return View(new DataEditForm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Create([FromForm] DataEditForm form)
    {
        if (ModelState.IsValid)
        {
            var id = await DataService.InsertAsync(form.Name, form.Value!.Value);
            if (id is not null)
            {
                TempData.SetMessage("データを作成しました");

                return RedirectToAction(nameof(List), new { Go = true });
            }

            ModelState.AddModelError(nameof(form.Name), Messages.DuplicateName);
        }

        return View(form);
    }

    //--------------------------------------------------------------------------------
    // Edit
    //--------------------------------------------------------------------------------

    [HttpGet("~/[controller]/[action]/{id:long}")]
    public async ValueTask<IActionResult> Edit(long id)
    {
        var entity = await DataService.QueryAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        return View(DataMapper.ToForm(entity));
    }

    [HttpPost("~/[controller]/[action]/{id:long}")]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Edit(long id, [FromForm] DataEditForm form)
    {
        if (ModelState.IsValid)
        {
            switch (await DataService.UpdateAsync(id, form.Name, form.Value!.Value))
            {
                case DataWriteStatus.Success:
                    TempData.SetMessage("データを更新しました");

                    return RedirectToAction(nameof(List), new { Go = true });
                case DataWriteStatus.Duplicate:
                    ModelState.AddModelError(nameof(form.Name), Messages.DuplicateName);
                    break;
                case DataWriteStatus.NotFound:
                    return NotFound();
            }
        }

        return View(form);
    }

    //--------------------------------------------------------------------------------
    // Export
    //--------------------------------------------------------------------------------

    [HttpGet]
    public IActionResult Export([FromQuery] DataListCondition c)
    {
        // 検索条件・ソートを引き継いでストリーミング出力
        return new PushStreamResult("text/csv", "data.csv", async stream =>
        {
            var cancellationToken = HttpContext.RequestAborted;
            await using var writer = new StreamWriter(stream, new UTF8Encoding(true));
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(DataService.QueryExportEnumerable(c.Name, c.Sort, c.Desc, cancellationToken), cancellationToken);
        });
    }

    //--------------------------------------------------------------------------------
    // Report
    //--------------------------------------------------------------------------------

    [HttpGet]
    public async ValueTask<IActionResult> Invoice([FromServices] InvoiceReportBuilder reportBuilder)
    {
        // 帳票生成前にデータ有無を判定(空PDFを作らない)
        var entities = await DataService.QueryAllAsync();
        if (entities.Count == 0)
        {
            TempData.SetMessage("出力対象のデータがありません");

            return RedirectToAction(nameof(List));
        }

        return File(reportBuilder.Build(entities), "application/pdf", "invoice.pdf");
    }

    //--------------------------------------------------------------------------------
    // Delete
    //--------------------------------------------------------------------------------

    [HttpPost("~/[controller]/[action]/{id:long}")]
    [Authorize(Policy = Policies.Administrator)]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Delete(long id)
    {
        if (!await DataService.DeleteAsync(id))
        {
            return NotFound();
        }

        TempData.SetMessage("データを削除しました");

        return RedirectToAction(nameof(List), new { Go = true });
    }
}
