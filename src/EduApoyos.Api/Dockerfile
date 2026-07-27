# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY EduApoyos.sln ./
COPY src/EduApoyos.Domain/EduApoyos.Domain.csproj src/EduApoyos.Domain/
COPY src/EduApoyos.Application/EduApoyos.Application.csproj src/EduApoyos.Application/
COPY src/EduApoyos.Infrastructure/EduApoyos.Infrastructure.csproj src/EduApoyos.Infrastructure/
COPY src/EduApoyos.Api/EduApoyos.Api.csproj src/EduApoyos.Api/

RUN dotnet restore src/EduApoyos.Api/EduApoyos.Api.csproj

COPY src/ src/

RUN dotnet publish src/EduApoyos.Api/EduApoyos.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# curl = healthcheck; fonts = QuestPDF (DejaVu) on Linux containers
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl fontconfig fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EduApoyos.Api.dll"]
