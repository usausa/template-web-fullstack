namespace Template.WebApp;

using AngleSharp.Html.Parser;

public sealed class ScreenTest : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public ScreenTest(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task LoginPageShowsForm()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/account/login", UriKind.Relative), TestContext.Current.CancellationToken);
        var document = await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(document.QuerySelector("input#Name"));
        Assert.NotNull(document.QuerySelector("input#Password"));
        Assert.NotNull(document.QuerySelector("input[name='__RequestVerificationToken']"));
    }

    [Fact]
    public async Task LoginShowsDashboard()
    {
        // Arrange
        var client = factory.CreateClient();
        var loginResponse = await client.GetAsync(new Uri("/account/login", UriKind.Relative), TestContext.Current.CancellationToken);
        var loginDocument = await new HtmlParser().ParseDocumentAsync(await loginResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var token = loginDocument.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;

        // Act
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "admin",
            ["Password"] = "admin",
            ["__RequestVerificationToken"] = token
        });
        var response = await client.PostAsync(new Uri("/account/login", UriKind.Relative), content, TestContext.Current.CancellationToken);
        var document = await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("ダッシュボード", document.Title, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NotFoundShowsErrorPage()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/nonexist", UriKind.Relative), TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("対象が存在しません", content, StringComparison.Ordinal);
    }
}
