# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["jobzilla-net/jobzilla-net.csproj", "jobzilla-net/"]
COPY ["jobzilla-net.Application/jobzilla-net.Application.csproj", "jobzilla-net.Application/"]
COPY ["jobzilla-net.Core/jobzilla-net.Core.csproj", "jobzilla-net.Core/"]
COPY ["jobzilla-net.Infrasture/jobzilla-net.Infrasture.csproj", "jobzilla-net.Infrasture/"]
COPY ["jobzilla-net.Persistence/jobzilla-net.Persistence.csproj", "jobzilla-net.Persistence/"]
RUN dotnet restore "./jobzilla-net/jobzilla-net.csproj"
COPY . .
WORKDIR "/src/jobzilla-net"
RUN dotnet build "./jobzilla-net.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./jobzilla-net.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "jobzilla-net.dll"]
