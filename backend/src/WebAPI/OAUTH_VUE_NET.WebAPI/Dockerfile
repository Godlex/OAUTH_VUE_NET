# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/WebAPI/OAUTH_VUE_NET.WebAPI/OAUTH_VUE_NET.WebAPI.csproj backend/src/WebAPI/OAUTH_VUE_NET.WebAPI/
COPY backend/src/BLL/OAUTH_VUE_NET.BLL/OAUTH_VUE_NET.BLL.csproj backend/src/BLL/OAUTH_VUE_NET.BLL/
COPY backend/src/Data/OAUTH_VUE_NET.Data/OAUTH_VUE_NET.Data.csproj backend/src/Data/OAUTH_VUE_NET.Data/
RUN dotnet restore backend/src/WebAPI/OAUTH_VUE_NET.WebAPI/OAUTH_VUE_NET.WebAPI.csproj

COPY backend/ backend/
WORKDIR /src/backend/src/WebAPI/OAUTH_VUE_NET.WebAPI
RUN dotnet build OAUTH_VUE_NET.WebAPI.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish OAUTH_VUE_NET.WebAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

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
ENV ASPNETCORE_HTTP_LOGGING__LOGGINGLEVEL=Information

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OAUTH_VUE_NET.WebAPI.dll"]
