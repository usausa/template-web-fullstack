namespace Template.WebApp;

using System.Text.RegularExpressions;

using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

public sealed class LoginTests : PageTest
{
    [Fact]
    public async Task LoginShowsDashboardPage()
    {
        // Arrange
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        // Act
        await Page.GotoAsync(factory.ServerAddress + "/");
        await Expect(Page).ToHaveURLAsync(new Regex(".*/account/login.*"));

        await Page.FillAsync("#Name", "admin");
        await Page.FillAsync("#Password", "admin");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン" }).ClickAsync();

        // Assert
        await Expect(Page).ToHaveTitleAsync(new Regex("ダッシュボード.*"));
    }

    [Fact]
    public async Task LoginWithWrongPasswordShowsError()
    {
        // Arrange
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        // Act
        await Page.GotoAsync(factory.ServerAddress + "/account/login");
        await Page.FillAsync("#Name", "admin");
        await Page.FillAsync("#Password", "wrong");
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "ログイン" }).ClickAsync();

        // Assert
        await Expect(Page.GetByText("ログインに失敗しました")).ToBeVisibleAsync();
    }
}
