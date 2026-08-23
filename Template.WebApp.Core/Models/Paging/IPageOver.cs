namespace Template.WebApp.Models.Paging;

public interface IPageOver
{
    int TotalPage { get; }

    bool IsOver { get; }
}
