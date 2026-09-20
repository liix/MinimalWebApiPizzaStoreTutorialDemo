FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PizzaStore/PizzaStore.csproj ./PizzaStore/
RUN dotnet restore PizzaStore/PizzaStore.csproj

COPY PizzaStore/ ./PizzaStore/
RUN dotnet publish PizzaStore/PizzaStore.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PizzaStore.dll"]