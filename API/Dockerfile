FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ShisaKanjis/ShisaKanjis.csproj", "ShisaKanjis/"]
RUN dotnet restore "ShisaKanjis/ShisaKanjis.csproj"
COPY . .
WORKDIR "/src/ShisaKanjis"
RUN dotnet build "ShisaKanjis.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ShisaKanjis.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ShisaKanjis.dll"]