using System;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;

namespace AuthorizationSample.Tools;

[McpServerToolType]
[Authorize]
public class OrderTools
{
    [McpServerTool]
    [Authorize(Roles = "Admin")]
    public string DeleteOrder(int id)
    {
        return "Deleted";
    }

    [McpServerTool]
    [Authorize(Policy = "CanRefund")]
    public string RefundOrder(int id)
    {
        return "Refunded";
    }

    [McpServerTool]
    [AllowAnonymous]
    public string Status() => "Healthy";
}
