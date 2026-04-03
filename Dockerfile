# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution và csproj
COPY UrlShortener.sln .
COPY UrlShortener.Api/UrlShortener.Api.csproj UrlShortener.Api/
COPY UrlShortener.Tests/UrlShortener.Tests.csproj UrlShortener.Tests/

# Restore
RUN dotnet restore

# Copy code
COPY . .

# Build và Publish 
RUN dotnet publish UrlShortener.Api/UrlShortener.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]