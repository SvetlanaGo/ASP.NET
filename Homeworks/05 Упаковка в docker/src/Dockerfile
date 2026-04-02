FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/*.sln* ./
COPY src/PromoCodeFactory.Core/*.csproj PromoCodeFactory.Core/
COPY src/PromoCodeFactory.DataAccess/*.csproj PromoCodeFactory.DataAccess/
COPY src/PromoCodeFactory.WebHost/*.csproj PromoCodeFactory.WebHost/
COPY src/PromoCodeFactory.UnitTests/*.csproj PromoCodeFactory.UnitTests/
RUN dotnet restore

COPY src/ .
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build

FROM base AS final
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PromoCodeFactory.WebHost.dll"]