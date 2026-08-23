namespace Template.WebApp.Host.Components.TagHelpers;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("app-message")]
public sealed class MessageTagHelper : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = string.Empty;

        if (!ViewContext.TempData.TryGetValue("Message", out var message))
        {
            return;
        }

        var builder = new StringBuilder();
        builder.Append("<div class=\"alert alert-success alert-dismissible\" role=\"alert\">");
        builder.Append(((string)message!).Replace("\n", "<br />", StringComparison.InvariantCulture));
        builder.Append("<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>");
        builder.Append("</div>");
        output.Content.SetHtmlContent(builder.ToString());
    }
}
