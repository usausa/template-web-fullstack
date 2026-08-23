namespace Template.WebApp.Host.Components.TagHelpers;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using Smart.AspNetCore;

[HtmlTargetElement(Attributes = DirtyAttributeName)]
public sealed class DirtyTagHelper : TagHelper
{
    private const string DirtyAttributeName = "i-dirty";

    private const string DirtyPostAttributeName = "i-dirty-post";

    [HtmlAttributeName(DirtyAttributeName)]
    public bool Dirty { get; set; }

    [HtmlAttributeName(DirtyPostAttributeName)]
    public bool DirtyPost { get; set; } = true;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // POST再描画(検証エラー等)は入力済みとして扱う
        var value = Dirty || (DirtyPost && ViewContext.IsPost());
        output.Attributes.Add("data-dirty", value ? "true" : "false");
    }
}
