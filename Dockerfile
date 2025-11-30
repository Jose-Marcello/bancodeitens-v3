#------------------------------------------------------------------
# Estágio 1: Build da Aplicação ASP.NET Core (BUILD)
#------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Cria um diretório para cache NuGet
RUN mkdir -p /nuget_cache

# 2. Copia todos os arquivos
COPY . .

# 3. Restaura e Publica TUDO em um único comando, FORÇANDO o caminho do cache.
# Esta é a correção crítica: /p:RestorePackagesPath direciona a restauração
# e a busca por pacotes para um local consistente e acessível.
RUN dotnet publish "src/BancoItens.Api/BancoItens.Api.csproj" \
    -c Release -o /publish \
    /p:UseAppHost=false \
    /p:RuntimeIdentifier=linux-x64 \
    /p:RestorePackagesPath=/nuget_cache

#------------------------------------------------------------------
# Estágio 2: Imagem de Produção Final (RUNTIME)
#------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copia os arquivos publicados
COPY --from=build /publish .

# Comando final para rodar a aplicação
CMD ["dotnet", "BancoItens.Api.dll", "--urls", "http://0.0.0.0:8080"]