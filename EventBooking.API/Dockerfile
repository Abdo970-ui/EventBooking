# مرحلة البناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات الـ Solution والـ Projects كلها لجذر الـ /src في الحاوية
COPY ["EventBooking.sln", "./"]
COPY ["EventBooking.API/EventBooking.API.csproj", "EventBooking.API/"]
COPY ["EventBooking.Application/EventBooking.Application.csproj", "EventBooking.Application/"]
COPY ["EventBooking.Domain/EventBooking.Domain.csproj", "EventBooking.Domain/"]
COPY ["EventBooking.Infrastructure/EventBooking.Infrastructure.csproj", "EventBooking.Infrastructure/"]

# استعادة الحزم لجميع المشاريع
RUN dotnet restore "EventBooking.API/EventBooking.API.csproj"

# نسخ باقي ملفات المشروع بالكامل
COPY . .
WORKDIR "/src/EventBooking.API"
RUN dotnet build "EventBooking.API.csproj" -c Release -o /app/build

# مرحلة النشر
FROM build AS publish
RUN dotnet publish "EventBooking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# المرحلة النهائية لتشغيل التطبيق
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventBooking.API.dll"]