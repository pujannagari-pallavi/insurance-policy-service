FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PolicyService.API/PolicyService.API.csproj src/PolicyService.API/
COPY src/PolicyService.Application/PolicyService.Application.csproj src/PolicyService.Application/
COPY src/PolicyService.Domain/PolicyService.Domain.csproj src/PolicyService.Domain/
COPY src/PolicyService.Infrastructure/PolicyService.Infrastructure.csproj src/PolicyService.Infrastructure/
RUN dotnet restore src/PolicyService.API/PolicyService.API.csproj

COPY src/ src/
RUN dotnet publish src/PolicyService.API/PolicyService.API.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "if [ -n \"$PORT\" ]; then exec dotnet PolicyService.API.dll --urls http://0.0.0.0:$PORT; else exec dotnet PolicyService.API.dll; fi"]