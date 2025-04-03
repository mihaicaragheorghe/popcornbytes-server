using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PopcornBytes.Api.Extensions;

public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Task WriteResponse(
        HttpContext context,
        HealthReport report)
    {
        string json = JsonSerializer.Serialize(
            new
            {
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Info = report.Entries
                    .Select(e =>
                        new
                        {
                            e.Key,
                            e.Value.Description,
                            e.Value.Duration,
                            Status = Enum.GetName(e.Value.Status),
                            Error = e.Value.Exception?.Message,
                            e.Value.Data
                        })
                    .ToList()
            },
            SerializerOptions);

        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(json);
    }
}