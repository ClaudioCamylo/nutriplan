FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/NutriPlan.Web -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet NutriPlan.Web.dll
