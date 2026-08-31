# Використовуємо образ для збірки 
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копіюємо абсолютно всі папки та файли репозиторію
COPY . .

# Переходимо в папку BLAZOR-проєкту (замініть на вашу точну назву папки)
WORKDIR /src/SchoolJournal.Blazor
RUN dotnet publish "SchoolJournal.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false -p:NoWarn=1591

# Створюємо фінальний образ для запуску на сервері
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_hostBuilder__reloadConfigOnChange=false
COPY --from=build /app/publish .

# Запускаємо DLL саме Blazor-проєкту
ENTRYPOINT ["dotnet", "SchoolJournal.Blazor.dll"]