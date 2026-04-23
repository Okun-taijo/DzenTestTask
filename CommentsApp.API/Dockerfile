FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CommentsApp.sln", "./"]
COPY ["CommentsApp.API/CommentsApp.API.csproj", "CommentsApp.API/"]
COPY ["CommentsApp.Application/CommentsApp.Application.csproj", "CommentsApp.Application/"]
COPY ["CommentsApp.Infrastructure/CommentsApp.Infrastructure.csproj", "CommentsApp.Infrastructure/"]

RUN dotnet restore "CommentsApp.API/CommentsApp.API.csproj"

COPY . .
RUN dotnet publish "CommentsApp.API/CommentsApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 libfreetype6 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CommentsApp.API.dll"]
