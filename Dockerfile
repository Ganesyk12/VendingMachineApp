# Base stage for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=6080
EXPOSE 6080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VendingMachineApp.csproj", "."]
RUN dotnet restore "./VendingMachineApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "VendingMachineApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VendingMachineApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VendingMachineApp.dll"]
