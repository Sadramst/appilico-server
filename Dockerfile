# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY nuget.config .
COPY src/AppilicoShopServer.Domain/AppilicoShopServer.Domain.csproj src/AppilicoShopServer.Domain/
COPY src/AppilicoShopServer.DataAccess/AppilicoShopServer.DataAccess.csproj src/AppilicoShopServer.DataAccess/
COPY src/AppilicoShopServer.Business/AppilicoShopServer.Business.csproj src/AppilicoShopServer.Business/
COPY src/AppilicoShopServer.API/AppilicoShopServer.API.csproj src/AppilicoShopServer.API/

# Restore dependencies
RUN dotnet restore src/AppilicoShopServer.API/AppilicoShopServer.API.csproj

# Copy source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/AppilicoShopServer.API/AppilicoShopServer.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/* \
	&& addgroup --system appgroup \
	&& adduser --system --ingroup appgroup appuser

# Copy published app
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/Logs && chown -R appuser:appgroup /app/Logs

# Switch to non-root user
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 CMD curl -fsS "http://localhost:${PORT:-8080}/health/live" || exit 1

ENTRYPOINT ["dotnet", "AppilicoShopServer.API.dll"]
