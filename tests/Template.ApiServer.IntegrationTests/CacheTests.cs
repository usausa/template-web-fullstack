namespace Template.ApiServer;

using System.Net.Http.Headers;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

using Template.ApiServer.Host.Endpoints;

// 一覧がValkey(L2)へ載ること、更新系で無効化されて最新が返ることを確認する
public sealed class CacheTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory factory;

    public CacheTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    [ContainerFact]
    public async Task ListIsCachedAndInvalidatedByWrite()
    {
        // Arrange
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync(new Uri("/api/v1/auth/login", UriKind.Relative), new LoginRequest { Id = "test", Password = "test" }, TestContext.Current.CancellationToken);
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken))!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await client.PostAsJsonAsync(new Uri("/api/v1/data", UriKind.Relative), new DataCreateRequest { Name = "CacheItem", Value = 1 }, TestContext.Current.CancellationToken);
        var created = await create.Content.ReadFromJsonAsync<DataCreateResponse>(TestContext.Current.CancellationToken);

        // Act
        var first = await client.GetFromJsonAsync<DataListResponse>(new Uri("/api/v1/data?name=CacheItem", UriKind.Relative), TestContext.Current.CancellationToken);
        // HybridCacheはL2へ利用側のキーそのままで保存する(DataServiceのキー形式)
        var cached = await factory.Services.GetRequiredService<IDistributedCache>().GetAsync("data:list:CacheItem:Id:False:0:20", TestContext.Current.CancellationToken);
        var update = await client.PutAsJsonAsync(new Uri($"/api/v1/data/{created!.Id}", UriKind.Relative), new DataUpdateRequest { Name = "CacheItem", Value = 2 }, TestContext.Current.CancellationToken);
        var second = await client.GetFromJsonAsync<DataListResponse>(new Uri("/api/v1/data?name=CacheItem", UriKind.Relative), TestContext.Current.CancellationToken);
        var delete = await client.DeleteAsync(new Uri($"/api/v1/data/{created.Id}", UriKind.Relative), TestContext.Current.CancellationToken);
        var third = await client.GetFromJsonAsync<DataListResponse>(new Uri("/api/v1/data?name=CacheItem", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, Assert.Single(first!.Items).Value);
        Assert.NotNull(cached);
        update.EnsureSuccessStatusCode();
        Assert.Equal(2, Assert.Single(second!.Items).Value);
        delete.EnsureSuccessStatusCode();
        Assert.Empty(third!.Items);
    }

    [ContainerFact]
    public async Task PublicEndpointIsServedFromOutputCache()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var first = await client.GetAsync(new Uri("/api/v1/test/time", UriKind.Relative), TestContext.Current.CancellationToken);
        var second = await client.GetAsync(new Uri("/api/v1/test/time", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.False(first.Headers.Contains("Age"));
        Assert.True(second.Headers.Contains("Age"));
        Assert.Equal(await first.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), await second.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
