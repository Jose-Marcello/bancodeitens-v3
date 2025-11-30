#------------------------------------------------------------------
# Estágio 1: Build da Aplicação ASP.NET Core (BUILD)
#------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Copia todos os arquivos (Já fizemos este commit)
COPY . .

# 2. Restaura explicitamente a Solução
RUN dotnet restore BancoDeItens_V3.sln

# 3. NOVO: Força a compilação de toda a Solução ANTES da publicação.
# Isso garante que todas as referências internas sejam resolvidas e construídas.
# Usamos --no-restore pois já rodamos o restore no passo anterior.
RUN dotnet build BancoDeItens_V3.sln --no-restore -c Release

# 4. Publica apenas o projeto da API, usando os binários construídos.
# Usamos --no-build para não compilar novamente, apenas empacotar o que já foi construído.
RUN dotnet publish "src/BancoItens.Api/BancoItens.Api.csproj" --no-build -c Release -o /publish /p:UseAppHost=false /p:RuntimeIdentifier=linux-x64

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