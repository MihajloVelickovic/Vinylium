FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# like mkdir -p {name} on the container
WORKDIR /server/app

# copies the csproj to /server/Vinylium on the container and restores
# the project
COPY ["app/app.csproj" , "."]
RUN dotnet restore "app.csproj"

# copies the rest of the files
COPY ["app", "."]
RUN dotnet build "app.csproj" -c Release -o /app/build

# https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
FROM build AS publish
RUN dotnet publish "app.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
EXPOSE 1738
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "app.dll"]

