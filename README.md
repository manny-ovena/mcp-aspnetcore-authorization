# MCP ASP.NET Core Authorization

An educational .NET 10 solution demonstrating role-based and policy-based
authorization for Model Context Protocol (MCP) tools hosted in ASP.NET Core.

**This repository is for educational purposes.** It is a learning sample, not a
production-ready authentication or authorization solution. Review and adapt the
security configuration before using these patterns in a real application.

## What it demonstrates

- Integrating the MCP SDK's authorization filters through `WithAuthorization()`.
- Applying `[Authorize]`, role requirements, named policies, and `[AllowAnonymous]`
  to MCP tools.
- Filtering `tools/list` results based on the caller's permissions.
- Enforcing authorization on `tools/call`, including calls made without prior
  discovery.

The sample exposes MCP at `/mcp` and defines three tools:

| Tool | Requirement |
|------|-------------|
| `delete_order` | Authenticated user with the `Admin` role |
| `refund_order` | Authenticated user with the `permission=orders.refund` claim (`CanRefund` policy) |
| `status` | Anonymous access allowed |

The `Admin` role does not automatically grant refund permission.

## Solution layout

| Project | Purpose |
|---------|---------|
| `src/Mcp.AspNetCore.Authorization` | Builder extension wrapping the MCP SDK's authorization filters |
| `samples/AuthorizationSample` | ASP.NET Core MCP server with protected tools |
| `tests/Mcp.AspNetCore.Authorization.Tests` | Library registration tests |
| `tests/AuthorizationSample.Tests` | HTTP integration tests for the sample |

## Build and test

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then run:

```bash
dotnet build Mcp.AspNetCore.Authorization.slnx
dotnet test Mcp.AspNetCore.Authorization.slnx
```

The integration tests use `WebApplicationFactory` and an MCP HTTP client to test
both `tools/list` and direct `tools/call` requests for anonymous users,
authenticated users without permissions, admins, refunders, and users with both
permissions.

Authentication is replaced with a **test-only** handler in the integration test
host. These tests exercise authorization, not JWT signature, issuer, audience,
or expiration validation. The test identity header is not supported by the
sample application itself.

## Run the sample

```bash
dotnet run --project samples/AuthorizationSample --launch-profile http
```

The MCP endpoint is available at `http://localhost:5248/mcp`. Use an MCP-compatible
client to initialize a session, list tools, and call them.

The sample registers JWT bearer authentication but does not include an identity
provider or JWT validation configuration. Configure a trusted issuer, audience,
and signing-key validation before testing authenticated calls outside the
integration test host. Do not put credentials or signing secrets in source control.

## License

Licensed under the [MIT License](LICENSE).
