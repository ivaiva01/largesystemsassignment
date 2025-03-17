FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["apiCleaner/apiCleaner.csproj", "apiCleaner/"]
RUN dotnet restore "apiCleaner/apiCleaner.csproj"
COPY . .
WORKDIR "/src/apiCleaner"
RUN dotnet build "apiCleaner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apiCleaner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "apiCleaner.dll"]
