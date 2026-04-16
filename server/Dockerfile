FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# like mkdir -p {name} on the container
WORKDIR /server/app

# copies the csproj to /server/Vinylium on the container and restores
# the project
COPY ["app/Vinylium.csproj" , "."]
RUN dotnet restore "Vinylium.csproj"

# copies the rest of the files
COPY ["app", "."]
RUN dotnet build "Vinylium.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vinylium.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
EXPOSE 1738
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vinylium.dll"]

