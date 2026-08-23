namespace Template.WebApp.Host.Areas.Api.Models;

public sealed record DataListResponse(int Total, int Page, int Size, IReadOnlyList<DataResponse> Items);
