# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["FlexiBoard.API/FlexiBoard.API.csproj", "FlexiBoard.API/"]
COPY ["FlexiBoard.Application/FlexiBoard.Application.csproj", "FlexiBoard.Application/"]
COPY ["FlexiBoard.Infrastructure/FlexiBoard.Infrastructure.csproj", "FlexiBoard.Infrastructure/"]
COPY ["FlexiBoard.Domain/FlexiBoard.Domain.csproj", "FlexiBoard.Domain/"]

# Restore dependencies
RUN dotnet restore "FlexiBoard.API/FlexiBoard.API.csproj"

# Copy all source code
COPY . .

# Build
RUN dotnet build "FlexiBoard.API/FlexiBoard.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FlexiBoard.API/FlexiBoard.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "FlexiBoard.API.dll"]
