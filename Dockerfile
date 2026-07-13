# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so dependency layers cache independently of source changes.
COPY RpgWorkspace.sln ./
COPY src/RpgWorkspace.Domain/RpgWorkspace.Domain.csproj src/RpgWorkspace.Domain/
COPY src/RpgWorkspace.Application/RpgWorkspace.Application.csproj src/RpgWorkspace.Application/
COPY src/RpgWorkspace.Infrastructure/RpgWorkspace.Infrastructure.csproj src/RpgWorkspace.Infrastructure/
COPY src/RpgWorkspace.Api/RpgWorkspace.Api.csproj src/RpgWorkspace.Api/
RUN dotnet restore RpgWorkspace.sln

COPY src/ src/
RUN dotnet publish src/RpgWorkspace.Api/RpgWorkspace.Api.csproj -c Release -o /app/publish -p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RpgWorkspace.Api.dll"]
