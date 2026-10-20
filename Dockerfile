FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EventBooking.sln", "./"]
COPY ["EventBooking.API/EventBooking.API.csproj", "EventBooking.API/"]
COPY ["EventBooking.Application/EventBooking.Application.csproj", "EventBooking.Application/"]
COPY ["EventBooking.Domain/EventBooking.Domain.csproj", "EventBooking.Domain/"]
COPY ["EventBooking.Infrastructure/EventBooking.Infrastructure.csproj", "EventBooking.Infrastructure/"]

RUN dotnet restore "EventBooking.API/EventBooking.API.csproj"

COPY . .
WORKDIR "/src/EventBooking.API"
RUN dotnet publish "EventBooking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventBooking.API.dll"]