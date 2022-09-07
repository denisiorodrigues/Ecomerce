#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/services/Ecomerce.Pedido.API/Ecomerce.Pedido.API.csproj", "src/services/Ecomerce.Pedido.API/"]
RUN dotnet restore "src/services/Ecomerce.Pedido.API/Ecomerce.Pedido.API.csproj"
COPY . .
WORKDIR "/src/src/services/Ecomerce.Pedido.API"
RUN dotnet build "Ecomerce.Pedido.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecomerce.Pedido.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecomerce.Pedido.API.dll"]