using FluentAssertions;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace AuthorizationSample.Tests;

public sealed class OrderToolsAuthorizationTests : IClassFixture<OrderToolsApplicationFactory>
{
    private readonly OrderToolsApplicationFactory _factory;

    public OrderToolsAuthorizationTests(OrderToolsApplicationFactory factory)
    {
        _factory = factory;
    }

    public static TheoryData<string, bool, bool> Identities => new()
    {
        { "anonymous", false, false },
        { "user", false, false },
        { "admin", true, false },
        { "refunder", false, true },
        { "admin-refunder", true, true },
    };

    [Theory]
    [MemberData(nameof(Identities))]
    public async Task ListTools_OnlyIncludesAuthorizedTools(string identity, bool canDelete, bool canRefund)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var httpClient = CreateHttpClient(identity);
        await using var transport = CreateTransport(httpClient);
        await using var client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);

        var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);

        var expectedNames = new List<string> { "status" };
        if (canDelete)
        {
            expectedNames.Add("delete_order");
        }
        if (canRefund)
        {
            expectedNames.Add("refund_order");
        }

        tools.Select(tool => tool.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Theory]
    [MemberData(nameof(Identities))]
    public async Task CallTools_EnforcesAuthorizationWithoutPriorDiscovery(
        string identity, bool canDelete, bool canRefund)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var httpClient = CreateHttpClient(identity);
        await using var transport = CreateTransport(httpClient);
        await using var client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);

        await AssertCallAsync(client, "delete_order", canDelete, "Deleted", timeout.Token);
        await AssertCallAsync(client, "refund_order", canRefund, "Refunded", timeout.Token);
        await AssertCallAsync(client, "status", true, "Healthy", timeout.Token);
    }

    private HttpClient CreateHttpClient(string identity)
    {
        var client = _factory.CreateClient();
        if (identity != "anonymous")
        {
            client.DefaultRequestHeaders.Add("X-Test-Identity", identity);
        }
        return client;
    }

    private static HttpClientTransport CreateTransport(HttpClient httpClient) => new(
        new HttpClientTransportOptions
        {
            Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
            TransportMode = HttpTransportMode.StreamableHttp,
            EnableStandaloneGetStream = false,
        },
        httpClient);

    private static async Task AssertCallAsync(
        McpClient client, string toolName, bool allowed, string expectedText, CancellationToken cancellationToken)
    {
        var arguments = toolName == "status" ? null : new Dictionary<string, object?> { ["id"] = 123 };

        if (!allowed)
        {
            var exception = await Assert.ThrowsAsync<McpProtocolException>(async () =>
                await client.CallToolAsync(toolName, arguments, cancellationToken: cancellationToken));
            exception.Message.Should().Contain("Access forbidden");
            return;
        }

        var result = await client.CallToolAsync(toolName, arguments, cancellationToken: cancellationToken);

        result.IsError.Should().NotBe(true);
        result.Content.Should().ContainSingle()
            .Which.Should().BeOfType<TextContentBlock>()
            .Which.Text.Should().Be(expectedText);
    }
}
