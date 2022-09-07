#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/services/Ecomerce.Cliente.API/Ecomerce.Cliente.API.csproj", "src/services/Ecomerce.Cliente.API/"]
RUN dotnet restore "src/services/Ecomerce.Cliente.API/Ecomerce.Cliente.API.csproj"
COPY . .
WORKDIR "/src/src/services/Ecomerce.Cliente.API"
RUN dotnet build "Ecomerce.Cliente.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecomerce.Cliente.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecomerce.Cliente.API.dll"]