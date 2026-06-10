# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all project files preserving structure
COPY src/Pcf.ReceivingFromPartner/*.sln ./src/Pcf.ReceivingFromPartner/
COPY src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Core/*.csproj ./src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Core/
COPY src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.DataAccess/*.csproj ./src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.DataAccess/
COPY src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Integration/*.csproj ./src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.Integration/
COPY src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.UnitTests/*.csproj ./src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.UnitTests/
COPY src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/*.csproj ./src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/
COPY src/Pcf.Contracts/*.csproj ./src/Pcf.Contracts/

# Restore dependencies
RUN dotnet restore ./src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/Pcf.ReceivingFromPartner.WebHost.csproj

# Copy all source code
COPY src/ ./src/

# Publish the application
RUN dotnet publish ./src/Pcf.ReceivingFromPartner/Pcf.ReceivingFromPartner.WebHost/Pcf.ReceivingFromPartner.WebHost.csproj -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.ReceivingFromPartner.WebHost.dll"]