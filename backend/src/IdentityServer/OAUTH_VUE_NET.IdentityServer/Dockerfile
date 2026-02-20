# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/IdentityServer/OAUTH_VUE_NET.IdentityServer/OAUTH_VUE_NET.IdentityServer.csproj backend/src/IdentityServer/OAUTH_VUE_NET.IdentityServer/
RUN dotnet restore backend/src/IdentityServer/OAUTH_VUE_NET.IdentityServer/OAUTH_VUE_NET.IdentityServer.csproj

COPY backend/ backend/
WORKDIR /src/backend/src/IdentityServer/OAUTH_VUE_NET.IdentityServer
RUN dotnet build OAUTH_VUE_NET.IdentityServer.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish OAUTH_VUE_NET.IdentityServer.csproj -c Release -o /app/publish /p:UseAppHost=false

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Logging: all logs to stdout/stderr for Docker
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLED=false
ENV ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Information
ENV ASPNETCORE_LOGGING__LOGLEVEL__Microsoft=Information
ENV ASPNETCORE_LOGGING__LOGLEVEL__Microsoft.AspNetCore=Information
ENV ASPNETCORE_LOGGING__LOGLEVEL__Duende=Information
ENV ASPNETCORE_HTTP_LOGGING__LOGGINGLEVEL=Information

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OAUTH_VUE_NET.IdentityServer.dll"]
