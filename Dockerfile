FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/GameBacklog.Api/GameBacklog.Api.csproj src/GameBacklog.Api/
COPY src/GameBacklog.Domain/GameBacklog.Domain.csproj src/GameBacklog.Domain/
COPY src/GameBacklog.Infrastructure/GameBacklog.Infrastructure.csproj src/GameBacklog.Infrastructure/
RUN dotnet restore src/GameBacklog.Api/GameBacklog.Api.csproj

COPY src/ src/
RUN dotnet publish src/GameBacklog.Api/GameBacklog.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    USE_HTTPS_REDIRECTION=false
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "GameBacklog.Api.dll"]
