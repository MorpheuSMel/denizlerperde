FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PerdeProje/PerdeProje.csproj PerdeProje/
RUN dotnet restore PerdeProje/PerdeProje.csproj

COPY PerdeProje/ PerdeProje/
RUN dotnet publish PerdeProje/PerdeProje.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "PerdeProje.dll"]
