using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mcp.AspNetCore.Authorization;

public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithAuthorization(this IMcpServerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddAuthorizationFilters();
    }
}
