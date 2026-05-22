# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY nuget.config .
COPY src/Appilico.Server.Domain/Appilico.Server.Domain.csproj src/Appilico.Server.Domain/
COPY src/Appilico.Server.DataAccess/Appilico.Server.DataAccess.csproj src/Appilico.Server.DataAccess/
COPY src/Appilico.Server.Business/Appilico.Server.Business.csproj src/Appilico.Server.Business/
COPY src/Appilico.Server.API/Appilico.Server.API.csproj src/Appilico.Server.API/

# Restore dependencies
RUN dotnet restore src/Appilico.Server.API/Appilico.Server.API.csproj

# Copy source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/Appilico.Server.API/Appilico.Server.API.csproj -c Release -o /app/publish --no-restore

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

ENTRYPOINT ["dotnet", "Appilico.Server.API.dll"]
