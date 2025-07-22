# Builder stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src
ARG BUILD_CONFIGURATION=Release

# Copy project files (paths must match directory structure)
COPY ["Server/Server.csproj", "Server/"]
COPY ["Connection/CommenCompunents.csproj", "Connection/"]
RUN dotnet restore "Server/Server.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR /src/Server
RUN dotnet build "Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish "Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Server.dll"]