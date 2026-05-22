using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Appilico.Server.API.Swagger;

/// <summary>Adds common response metadata and correlation id support to OpenAPI operations.</summary>
public sealed class DefaultResponsesOperationFilter : IOperationFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        if (operation.Parameters.All(parameter => parameter.Name != "X-Correlation-Id"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Correlation-Id",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional request correlation id. The same value is returned in the response header.",
                Schema = new OpenApiSchema { Type = "string" }
            });
        }

        AddResponse(operation, "400", "Validation or malformed request.");
        AddResponse(operation, "401", "Authentication is required or the JWT is invalid.");
        AddResponse(operation, "403", "The authenticated user is not allowed to perform this action.");
        AddResponse(operation, "429", "Rate limit exceeded.");
        AddResponse(operation, "500", "Unexpected server error. Include X-Correlation-Id when reporting issues.");
    }

    private static void AddResponse(OpenApiOperation operation, string statusCode, string description)
    {
        if (!operation.Responses.ContainsKey(statusCode))
            operation.Responses.Add(statusCode, new OpenApiResponse { Description = description });
    }
}