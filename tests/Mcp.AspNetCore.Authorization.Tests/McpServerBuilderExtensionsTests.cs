using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Mcp.AspNetCore.Authorization.Tests;

public sealed class McpServerBuilderExtensionsTests
{
    [Fact]
    public void WithAuthorization_NullBuilder_Throws()
    {
        IMcpServerBuilder? builder = null;

#pragma warning disable CS8604 // Possible null reference argument.
        var exception = Assert.Throws<ArgumentNullException>(() => builder.WithAuthorization());
#pragma warning restore CS8604 // Possible null reference argument.

        exception.ParamName.Should().Be("builder");
    }

    [Fact]
    public void WithAuthorization_Returns_SameBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddMcpServer();

        var result = builder.WithAuthorization();

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithAuthorization_RegistersAuthorizationFilters()
    {
        var actualServices = new ServiceCollection();
        actualServices.AddMcpServer().WithAuthorization();

        var expectedServices = new ServiceCollection();
        expectedServices.AddMcpServer().AddAuthorizationFilters();

        actualServices
            .Select(descriptor => (descriptor.ServiceType, descriptor.Lifetime, descriptor.ImplementationType))
            .Should()
            .BeEquivalentTo(expectedServices.Select(descriptor =>
                (descriptor.ServiceType, descriptor.Lifetime, descriptor.ImplementationType)));
    }
}
