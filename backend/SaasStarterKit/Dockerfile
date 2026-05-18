FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SaasStarterKit/SaasStarterKit.API.csproj", "SaasStarterKit/"]
COPY ["SaasStarterKit.Application/SaasStarterKit.Application.csproj", "SaasStarterKit.Application/"]
COPY ["SaasStarterKit.Infrastructure/SaasStarterKit.Infrastructure.csproj", "SaasStarterKit.Infrastructure/"]
COPY ["SaasStarterKit.Domain/SaasStarterKit.Domain.csproj", "SaasStarterKit.Domain/"]
RUN dotnet restore "SaasStarterKit/SaasStarterKit.API.csproj"
COPY . .
WORKDIR "/src/SaasStarterKit"
RUN dotnet build "SaasStarterKit.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SaasStarterKit.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SaasStarterKit.API.dll"]