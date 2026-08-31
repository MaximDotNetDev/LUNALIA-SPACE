# Використовуємо образ для збірки 
# (Якщо у вас .NET 9, змініть 8.0 на 9.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо абсолютно всі папки та файли репозиторію
COPY . .

# Переходимо в папку бекенду і запускаємо збірку
WORKDIR /src/SchoolJournal.Api
RUN dotnet publish "SchoolJournal.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Створюємо фінальний образ для запуску на сервері
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SchoolJournal.Api.dll"]