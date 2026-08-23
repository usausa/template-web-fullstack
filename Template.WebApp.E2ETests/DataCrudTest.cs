namespace Template.WebApp;

using System.Text.RegularExpressions;

using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

public sealed class DataCrudTest : PageTest
{
    [Fact]
    public async Task CreateDataShowsInList()
    {
        // Arrange
        using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        await Page.GotoAsync(factory.ServerAddress + "/account/login");
        await Page.FillAsync("#Name", "admin");
        await Page.FillAsync("#Password", "admin");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン" }).ClickAsync();
        await Expect(Page).ToHaveTitleAsync(new Regex("ダッシュボード.*"));

        // Act
        await Page.GotoAsync(factory.ServerAddress + "/data/create");
        await Page.FillAsync("#Name", "E2EItem");
        await Page.FillAsync("#Value", "123");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "作成" }).ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(".*/data/list.*"));
        await Expect(Page.GetByText("データを作成しました")).ToBeVisibleAsync();
        await Expect(Page.Locator("tbody")).ToContainTextAsync("E2EItem");
    }
}
