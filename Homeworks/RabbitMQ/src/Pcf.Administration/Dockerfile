# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all project files preserving structure
COPY src/Pcf.Administration/*.sln ./src/Pcf.Administration/
COPY src/Pcf.Administration/Pcf.Administration.Core/*.csproj ./src/Pcf.Administration/Pcf.Administration.Core/
COPY src/Pcf.Administration/Pcf.Administration.DataAccess/*.csproj ./src/Pcf.Administration/Pcf.Administration.DataAccess/
COPY src/Pcf.Administration/Pcf.Administration.IntegrationTests/*.csproj ./src/Pcf.Administration/Pcf.Administration.IntegrationTests/
COPY src/Pcf.Administration/Pcf.Administration.WebHost/*.csproj ./src/Pcf.Administration/Pcf.Administration.WebHost/
COPY src/Pcf.Contracts/*.csproj ./src/Pcf.Contracts/

# Restore dependencies
RUN dotnet restore ./src/Pcf.Administration/Pcf.Administration.WebHost/Pcf.Administration.WebHost.csproj

# Copy all source code
COPY src/ ./src/

# Publish the application
RUN dotnet publish ./src/Pcf.Administration/Pcf.Administration.WebHost/Pcf.Administration.WebHost.csproj -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.Administration.WebHost.dll"]