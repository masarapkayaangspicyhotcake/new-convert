# Use the .NET 9.0 SDK Preview image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build

WORKDIR /app

# Copy the project files
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build and publish the app
RUN dotnet publish -c Release -o /app/out

# Use the ASP.NET 9.0 runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview

WORKDIR /app

# Copy built output from build stage
COPY --from=build /app/out .

# Start the app
ENTRYPOINT ["dotnet", "FinalProject.dll"]
