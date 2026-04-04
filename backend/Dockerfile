# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ./AnimStudio.API/ .
RUN dotnet restore && dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out ./
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "AnimStudio.API.dll"]