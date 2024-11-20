# Use the runtime image as the base for the final image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file(s) and restore dependencies
COPY ["BackendService/BackendService.csproj", "BackendService/"]
RUN dotnet restore "BackendService/BackendService.csproj"

# Copy the rest of the application files
COPY . .

# Build the application
WORKDIR "/src/BackendService"
RUN dotnet build "BackendService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application to prepare it for deployment
FROM build AS publish
RUN dotnet publish "BackendService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "BackendService.dll"]
