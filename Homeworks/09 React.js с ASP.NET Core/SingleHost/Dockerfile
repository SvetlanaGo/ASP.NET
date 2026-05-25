FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

RUN sed -i 's|http://security.ubuntu.com/ubuntu|http://archive.ubuntu.com/ubuntu|g' /etc/apt/sources.list.d/ubuntu.sources && \
    apt-get update && \
    apt-get install -y curl gnupg ca-certificates && \
    mkdir -p /etc/apt/keyrings && \
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg && \
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_20.x nodistro main" > /etc/apt/sources.list.d/nodesource.list && \
    apt-get update && \
    apt-get install -y nodejs && \
    apt-get clean && rm -rf /var/lib/apt/lists/*
	
COPY *.sln* ./
COPY Weather.Server/Weather.Server.csproj Weather.Server/
COPY weather.client/weather.client.esproj weather.client/
RUN dotnet restore

COPY weather.client/ weather.client/
WORKDIR /src/weather.client
RUN npm ci --ignore-scripts && npm run build

WORKDIR /src
COPY . .
WORKDIR /src/Weather.Server
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /app/publish --no-restore --no-build

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/weather.client/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Weather.Server.dll"]