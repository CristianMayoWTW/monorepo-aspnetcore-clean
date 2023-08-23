#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/AspNetCore.POC.WebApi/AspNetCore.POC.WebApi.csproj", "src/AspNetCore.POC.WebApi/"]
RUN dotnet restore "src/AspNetCore.POC.WebApi/AspNetCore.POC.WebApi.csproj"
COPY . .
WORKDIR "/src/src/AspNetCore.POC.WebApi"
RUN dotnet build "AspNetCore.POC.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetCore.POC.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore.POC.WebApi.dll"]