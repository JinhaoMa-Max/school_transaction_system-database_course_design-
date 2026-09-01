FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CampusTrade.Backend/CampusTrade.Backend.csproj CampusTrade.Backend/
RUN dotnet restore CampusTrade.Backend/CampusTrade.Backend.csproj

COPY . .
WORKDIR /src/CampusTrade.Backend
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CampusTrade.Backend.dll"]
