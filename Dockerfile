# Use official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MaritimeIQ.Platform.csproj", "."]
RUN dotnet restore "MaritimeIQ.Platform.csproj"
COPY . .
RUN dotnet build "MaritimeIQ.Platform.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MaritimeIQ.Platform.csproj" -c Release -o /app/publish --self-contained false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MaritimeIQ.Platform.dll"]
