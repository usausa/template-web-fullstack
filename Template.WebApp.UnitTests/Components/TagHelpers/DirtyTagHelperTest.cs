namespace Template.WebApp.Components.TagHelpers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

using Template.WebApp.Host.Components.TagHelpers;

public sealed class DirtyTagHelperTest
{
    [Fact]
    public void ProcessOnGetOutputsFalse()
    {
        // Arrange
        var helper = new DirtyTagHelper
        {
            ViewContext = CreateViewContext("GET")
        };
        var output = CreateOutput();

        // Act
        helper.Process(CreateContext(), output);

        // Assert
        Assert.Equal("false", output.Attributes["data-dirty"].Value);
    }

    [Fact]
    public void ProcessOnPostOutputsTrue()
    {
        // Arrange
        var helper = new DirtyTagHelper
        {
            ViewContext = CreateViewContext("POST")
        };
        var output = CreateOutput();

        // Act
        helper.Process(CreateContext(), output);

        // Assert
        Assert.Equal("true", output.Attributes["data-dirty"].Value);
    }

    private static ViewContext CreateViewContext(string method)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        return new ViewContext
        {
            HttpContext = httpContext
        };
    }

    private static TagHelperContext CreateContext() =>
        new([], new Dictionary<object, object>(), "test");

    private static TagHelperOutput CreateOutput() =>
        new("form", [], static (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
}
