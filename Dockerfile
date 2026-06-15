# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

COPY FinRegAuditPlatform.sln .
COPY src/FinReg.Domain/FinReg.Domain.csproj src/FinReg.Domain/
COPY src/FinReg.Application/FinReg.Application.csproj src/FinReg.Application/
COPY src/FinReg.Infrastructure/FinReg.Infrastructure.csproj src/FinReg.Infrastructure/
COPY src/FinReg.API/FinReg.API.csproj src/FinReg.API/
COPY tests/FinReg.Domain.Tests/FinReg.Domain.Tests.csproj tests/FinReg.Domain.Tests/
COPY tests/FinReg.Application.Tests/FinReg.Application.Tests.csproj tests/FinReg.Application.Tests/
COPY tests/FinReg.Integration.Tests/FinReg.Integration.Tests.csproj tests/FinReg.Integration.Tests/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/FinReg.API/FinReg.API.csproj

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/FinReg.API/FinReg.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FinReg.API.dll"]
