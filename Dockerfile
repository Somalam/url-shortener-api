FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution và các file dự án (đúng đường dẫn mới)
COPY UrlShortener.sln .
COPY UrlShortener.Api/UrlShortener.Api.csproj UrlShortener.Api/
COPY UrlShortener.Tests/UrlShortener.Tests.csproj UrlShortener.Tests/

# Restore toàn bộ thư viện
RUN dotnet restore

# Copy toàn bộ code vào và build
COPY . .
RUN dotnet publish UrlShortener.Api/UrlShortener.Api.csproj -c Release -o /app/publish

# Stage chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]