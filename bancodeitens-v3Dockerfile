#------------------------------------------------------------------
# Estágio 1: Build da Aplicação ASP.NET Core (BUILD)
#------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Cria o diretório para cache NuGet
RUN mkdir -p /nuget_cache

# 2. Copia a Solution e os Projetos
COPY BancoDeItens_V3.sln .
COPY src/ src/

# 3. Restaura explicitamente, FORÇANDO o RuntimeIdentifier AQUI.
# Isso garante que o project.assets.json seja criado para o target linux-x64.
RUN dotnet restore BancoDeItens_V3.sln \
    /p:RestorePackagesPath=/nuget_cache \
    /p:RestoreForce=true \
    /p:RuntimeIdentifier=linux-x64

# 4. Publica, usando --no-restore.
RUN dotnet publish "src/BancoItens.Api/BancoItens.Api.csproj" \
    -c Release -o /publish \
    /p:UseAppHost=false \
    /p:RuntimeIdentifier=linux-x64 \
    --no-restore # Isso é seguro agora, pois o assets.json está completo

# ... (Estágio 2: RUNTIME) ...