FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src


COPY ["CustomerService/CustomerService.csproj", "CustomerService/"]
COPY ["MicroserviceShopDemo.Common/MicroserviceShopDemo.Common.csproj", "MicroserviceShopDemo.Common/"]

RUN dotnet restore "CustomerService/CustomerService.csproj"

COPY . .
WORKDIR "/src/CustomerService"
RUN dotnet build "CustomerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerService.dll"]