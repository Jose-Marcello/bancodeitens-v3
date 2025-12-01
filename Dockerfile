#------------------------------------------------------------------
# Estágio 1: Build da Aplicação ASP.NET Core (BUILD)
#------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia todos os arquivos (a estrutura V4 limpa)
COPY . .

# Executa o restore e o publish no comando unico mais simples
RUN dotnet publish "src/BancoItens.Api/BancoItens.Api.csproj" -c Release -o /publish /p:UseAppHost=false

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