FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CRM SAP.csproj", "./"]
RUN dotnet restore "CRM SAP.csproj"
COPY . .
RUN dotnet build "CRM SAP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CRM SAP.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRM_ERP_UMG.dll"]
