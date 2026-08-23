namespace Template.WebApp.Host.Areas.Default.Models;

public sealed class DataListCondition : Pageable
{
    public string? Name { get; set; }

    public string? Sort { get; set; }

    public bool Desc { get; set; }

    public bool Go { get; set; }
}
