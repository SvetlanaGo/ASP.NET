FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY Weather.Server/Weather.Server.csproj Weather.Server/
RUN dotnet restore Weather.Server/Weather.Server.csproj

COPY Weather.Server/ Weather.Server/
WORKDIR /src/Weather.Server

RUN rm -rf obj bin && \
    dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Weather.Server.dll"]