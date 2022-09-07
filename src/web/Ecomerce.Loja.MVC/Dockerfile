#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/web/Ecomerce.Loja.MVC/Ecomerce.Loja.MVC.csproj", "src/web/Ecomerce.Loja.MVC/"]
RUN dotnet restore "src/web/Ecomerce.Loja.MVC/Ecomerce.Loja.MVC.csproj"
COPY . .
WORKDIR "/src/src/web/Ecomerce.Loja.MVC"
RUN dotnet build "Ecomerce.Loja.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecomerce.Loja.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecomerce.Loja.MVC.dll"]