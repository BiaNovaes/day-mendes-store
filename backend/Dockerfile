FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DayMendesStore.Domain/DayMendesStore.Domain.csproj DayMendesStore.Domain/
COPY DayMendesStore.Application/DayMendesStore.Application.csproj DayMendesStore.Application/
COPY DayMendesStore.Infrastructure/DayMendesStore.Infrastructure.csproj DayMendesStore.Infrastructure/
COPY DayMendesStore.API/DayMendesStore.API.csproj DayMendesStore.API/
RUN dotnet restore DayMendesStore.API/DayMendesStore.API.csproj

COPY . .
RUN dotnet publish DayMendesStore.API/DayMendesStore.API.csproj --configuration Release --output /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT dotnet DayMendesStore.API.dll
