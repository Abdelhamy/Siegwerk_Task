# Use the official .NET 9 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["Pricing.Api/Pricing.Api.csproj", "Pricing.Api/"]
COPY ["Pricing.Infrastructure/Pricing.Infrastructure.csproj", "Pricing.Infrastructure/"]
COPY ["Pricing.Application/Pricing.Application.csproj", "Pricing.Application/"]
COPY ["Pricing.Domain/Pricing.Domain.csproj", "Pricing.Domain/"]

# Restore dependencies
RUN dotnet restore "Pricing.Api/Pricing.Api.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/Pricing.Api"
RUN dotnet build "Pricing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Pricing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - use SDK image instead of runtime for EF tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Copy source code for EF migrations (needed for migrations to work)
COPY --from=build /src /src

# Install Entity Framework tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "Pricing.Api.dll"]