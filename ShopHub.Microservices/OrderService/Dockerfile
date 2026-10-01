# ── Build stage ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OrderService/OrderService.csproj", "OrderService/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "OrderService/OrderService.csproj"

COPY . .
WORKDIR "/src/OrderService"
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /app/data

ENV ConnectionStrings__Default="Data Source=/app/data/orders.db"
ENV ASPNETCORE_URLS="http://+:8080"

ENTRYPOINT ["dotnet", "OrderService.dll"]