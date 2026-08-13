FROM git.faramaham.com:5050/shared/docker_repository/dotnet/aspnet:9.0.1 AS base
WORKDIR /app

FROM git.faramaham.com:5050/shared/docker_repository/dotnet/sdk:9.0.102 AS build
WORKDIR /build
COPY ["./src/Api/Api.csproj", "./src/Api/"]
COPY ["./src/Application/Application.csproj", "./src/Application/"]
COPY ["./src/Domain/Domain.csproj", "./src/Domain/"]
COPY ["./src/Infrastructure/Infrastructure.csproj", "./src/Infrastructure/"]
COPY ["./Qalaat-Al-Dhaman.sln", "."]

RUN dotnet nuget disable source nuget.org

RUN dotnet nuget add source https://nuget.faramaham.com/v3/index.json --name FaramahamMirror

RUN dotnet restore

COPY ["." , "."]

FROM build AS publish
RUN dotnet publish ./src/Api/Api.csproj -o /publish /p:UseAppHost=false

FROM base AS final
COPY --from=publish /publish .
ENTRYPOINT ["dotnet", "Api.dll"]