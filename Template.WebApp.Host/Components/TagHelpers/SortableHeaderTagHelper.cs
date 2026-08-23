namespace Template.WebApp.Host.Components.TagHelpers;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Primitives;

using Smart.AspNetCore;

[HtmlTargetElement("th", Attributes = SortAttributeName)]
public sealed class SortableHeaderTagHelper : TagHelper
{
    private const string SortAttributeName = "sort-for";

    [HtmlAttributeName(SortAttributeName)]
    public string SortFor { get; set; } = default!;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var currentSort = ViewContext.GetQueryString("Sort");
        var currentDesc = String.Equals(ViewContext.GetQueryString("Desc"), "true", StringComparison.OrdinalIgnoreCase);
        var active = String.Equals(currentSort, SortFor, StringComparison.OrdinalIgnoreCase);

        // アクティブ列の再クリックで昇順/降順を反転。列変更時はページを先頭に戻す
        var query = ViewContext.ReplaceQuery(new Dictionary<string, StringValues>
        {
            ["Sort"] = SortFor,
            ["Desc"] = active && !currentDesc ? "true" : "false",
            ["Page"] = "1"
        });

        var childContent = await output.GetChildContentAsync();

        var anchor = new TagBuilder("a")
        {
            Attributes = { ["href"] = query.ToString() }
        };
        anchor.AddCssClass("text-decoration-none text-reset");
        anchor.InnerHtml.AppendHtml(childContent);
        if (active)
        {
            anchor.InnerHtml.Append(currentDesc ? " ▼" : " ▲");
        }

        output.Content.SetHtmlContent(anchor);
    }
}
