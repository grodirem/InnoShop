FROM mcr.microsoft.com/dotnet/sdk:9.0
COPY . /src
WORKDIR /src

# Покажем структуру
RUN echo "=== Solution structure ==="
RUN find . -name "*.csproj" -type f | head -20

# Покажем содержимое UserService.API.csproj
RUN echo "=== UserService.API.csproj ==="
RUN cat UserService/UserService.API/UserService.API.csproj

# Попробуем восстановить все решение
RUN echo "=== Restoring solution ==="
RUN dotnet restore InnoShop.sln

# Или конкретный проект
RUN echo "=== Restoring UserService.API ==="
WORKDIR /src/UserService/UserService.API
RUN dotnet restore