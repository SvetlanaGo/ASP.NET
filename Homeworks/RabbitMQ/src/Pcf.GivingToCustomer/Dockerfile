# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all project files preserving structure
COPY src/Pcf.GivingToCustomer/*.sln ./src/Pcf.GivingToCustomer/
COPY src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.Core/*.csproj ./src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.Core/
COPY src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.DataAccess/*.csproj ./src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.DataAccess/
COPY src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.Integration/*.csproj ./src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.Integration/
COPY src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.IntegrationTests/*.csproj ./src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.IntegrationTests/
COPY src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/*.csproj ./src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/
COPY src/Pcf.Contracts/*.csproj ./src/Pcf.Contracts/

# Restore dependencies
RUN dotnet restore ./src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/Pcf.GivingToCustomer.WebHost.csproj

# Copy all source code
COPY src/ ./src/

# Publish the application
RUN dotnet publish ./src/Pcf.GivingToCustomer/Pcf.GivingToCustomer.WebHost/Pcf.GivingToCustomer.WebHost.csproj -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.GivingToCustomer.WebHost.dll"]