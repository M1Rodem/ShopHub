# ── Build stage ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CatalogService/CatalogService.csproj", "CatalogService/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "CatalogService/CatalogService.csproj"

COPY . .
WORKDIR "/src/CatalogService"
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /app/data

ENV ConnectionStrings__Default="Data Source=/app/data/catalog.db"
ENV ASPNETCORE_URLS="http://+:8080"

ENTRYPOINT ["dotnet", "CatalogService.dll"]