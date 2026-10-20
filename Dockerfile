# 1. مرحلة البناء (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملف الـ Solution من المجلد الرئيسي (عن طريق الصعود خطوة للخلف ../) وباقي المشاريع
COPY ["../EventBooking.sln", "./"]
COPY ["EventBooking.API/EventBooking.API.csproj", "EventBooking.API/"]
COPY ["EventBooking.Application/EventBooking.Application.csproj", "EventBooking.Application/"]
COPY ["EventBooking.Domain/EventBooking.Domain.csproj", "EventBooking.Domain/"]
COPY ["EventBooking.Infrastructure/EventBooking.Infrastructure.csproj", "EventBooking.Infrastructure/"]

# استعادة الحزم لمشروع الـ API الأساسي
RUN dotnet restore "EventBooking.API/EventBooking.API.csproj"

# نسخ باقي الملفات والمشاريع
COPY . .
WORKDIR "/src/EventBooking.API"
RUN dotnet publish "EventBooking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. مرحلة التشغيل النهائية (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventBooking.API.dll"]