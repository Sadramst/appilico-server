using System.Net;
using AppilicoShopServer.API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppilicoShopServer.IntegrationTests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task ProductionExceptionResponse_DoesNotLeakInternalExceptionMessage()
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns(Environments.Production);
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("database secret detail leaked"),
            new Mock<ILogger<ExceptionHandlingMiddleware>>().Object,
            environment.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        context.Response.Headers.ContainsKey("X-Correlation-Id").Should().BeTrue();

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().NotContain("database secret detail leaked");
        body.Should().Contain("request could not be completed");
    }
}