namespace Template.WebApp.Host.Areas.Default.Controllers;

using Template.WebApp.Host.Infrastructure.Filters;
using Template.WebApp.Infrastructure.Storage;

[StorageExceptionFilter]
public sealed class FileController : BaseDefaultController
{
    private IStorage Storage { get; }

    public FileController(IStorage storage)
    {
        Storage = storage;
    }

    //--------------------------------------------------------------------------------
    // List
    //--------------------------------------------------------------------------------

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        var entries = await Storage.ListAsync(string.Empty, cancellationToken);
        return View(entries);
    }

    //--------------------------------------------------------------------------------
    // Upload
    //--------------------------------------------------------------------------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_485_760)]
    public async ValueTask<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if ((file is null) || (file.Length == 0))
        {
            TempData.SetMessage("ファイルを選択してください");

            return RedirectToAction(nameof(List));
        }

        await using (var stream = file.OpenReadStream())
        {
            await Storage.WriteAsync(Path.GetFileName(file.FileName), stream, cancellationToken);
        }

        TempData.SetMessage("ファイルをアップロードしました");

        return RedirectToAction(nameof(List));
    }

    //--------------------------------------------------------------------------------
    // Download
    //--------------------------------------------------------------------------------

    [HttpGet("~/[controller]/[action]/{**path}")]
    public async ValueTask<IActionResult> Download(string path, CancellationToken cancellationToken)
    {
        if (!await Storage.FileExistsAsync(path, cancellationToken))
        {
            return NotFound();
        }

        var stream = await Storage.ReadAsync(path, cancellationToken);
        return File(stream, "application/octet-stream", Path.GetFileName(path));
    }

    //--------------------------------------------------------------------------------
    // Delete
    //--------------------------------------------------------------------------------

    [HttpPost("~/[controller]/[action]/{**path}")]
    [Authorize(Policy = Policies.Administrator)]
    [ValidateAntiForgeryToken]
    public async ValueTask<IActionResult> Delete(string path, CancellationToken cancellationToken)
    {
        if (!await Storage.FileExistsAsync(path, cancellationToken))
        {
            return NotFound();
        }

        await Storage.DeleteAsync(path, cancellationToken);

        TempData.SetMessage("ファイルを削除しました");

        return RedirectToAction(nameof(List));
    }
}
