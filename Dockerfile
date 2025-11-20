# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["UmaibouAnalyzer.Api/UmaibouAnalyzer.Api.csproj", "UmaibouAnalyzer.Api/"]
RUN dotnet restore "UmaibouAnalyzer.Api/UmaibouAnalyzer.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/UmaibouAnalyzer.Api"
RUN dotnet build "UmaibouAnalyzer.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UmaibouAnalyzer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UmaibouAnalyzer.Api.dll"]
