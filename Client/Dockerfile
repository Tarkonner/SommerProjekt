# Builder stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src
ARG BUILD_CONFIGURATION=Release

# Copy project files (including dependencies)
COPY ["Client/Client.csproj", "Client/"]
COPY ["Connection/CommenCompunents.csproj", "Connection/"]
RUN dotnet restore "Client/Client.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR /src/Client
RUN dotnet build "Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish "Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

ENTRYPOINT ["dotnet", "Client.dll"]