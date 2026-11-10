
# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file and restore dependencies
COPY ["Mortgage Application API/Mortgage Application API.csproj", "Mortgage Application API/"]

RUN dotnet restore "Mortgage Application API/Mortgage Application API.csproj"

# Copy remaining source files
COPY . .

# Publish the application
WORKDIR "/src/Mortgage Application API"

RUN dotnet publish "Mortgage Application API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Listen on port 8080 inside the container
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Copy published application
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "Mortgage Application API.dll"]