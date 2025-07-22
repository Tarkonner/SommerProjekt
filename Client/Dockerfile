# Builder stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src
COPY ["Client.csproj", "./"]
RUN dotnet restore "Client.csproj"

# Copy the rest of the code
COPY . .
RUN dotnet build "Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish "Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "Client.dll"]