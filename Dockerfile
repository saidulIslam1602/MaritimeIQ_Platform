# Simple C# ASP.NET Core application for ACR Tasks demo
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AcrTasksDemo.csproj", "."]
RUN dotnet restore "AcrTasksDemo.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "AcrTasksDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcrTasksDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AcrTasksDemo.dll"]