FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src


COPY ["PaymentService/PaymentService.csproj", "PaymentService/"]
COPY ["MicroserviceShopDemo.Common/MicroserviceShopDemo.Common.csproj", "MicroserviceShopDemo.Common/"]

RUN dotnet restore "PaymentService/PaymentService.csproj"

COPY . .
WORKDIR "/src/PaymentService"
RUN dotnet build "PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]