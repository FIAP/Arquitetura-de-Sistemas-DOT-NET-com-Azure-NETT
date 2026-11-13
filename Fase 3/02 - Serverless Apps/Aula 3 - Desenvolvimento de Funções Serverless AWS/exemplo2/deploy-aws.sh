#!/bin/bash

# Script para criar pacote Lambda .NET 8
# Compila e empacota com arquitetura correta para AWS

set -e

echo "Criando pacote Lambda"

# Verificar se dotnet lambda tools está instalado
if ! dotnet tool list -g | grep -q "amazon.lambda.tools"; then
    echo "Instalando Amazon Lambda Tools..."
    dotnet tool install -g Amazon.Lambda.Tools
fi

# Limpar build anterior
rm -rf bin/Release/
rm -rf obj/

# Build com arquitetura correta
echo "Fazendo build..."
dotnet build --configuration Release --runtime linux-x64 --self-contained false

if [ $? -ne 0 ]; then
    echo "ERRO: Falha no build"
    exit 1
fi

# Criar pacote Lambda
echo "Criando pacote..."
dotnet lambda package \
    --configuration Release \
    --framework net8.0 \
    --runtime linux-x64 \
    --architecture x86_64 \
    --output-package bin/lambda-package.zip

if [ $? -ne 0 ]; then
    echo "ERRO: Falha ao criar pacote"
    exit 1
fi

echo "SUCESSO: Pacote criado em bin/lambda-package.zip"
echo ""
echo "CONFIGURACOES PARA UPLOAD NA AWS:"
echo "================================"
echo "Runtime: .NET 8"
echo "Arquitetura: x86_64" 
echo ""
echo "HANDLER (copie exatamente):"
echo "LambdaTestProject::LambdaTestProject.Function::FunctionHandler"
echo ""
echo "IMPORTANTE: O Handler deve ser exatamente como mostrado acima!"
echo "Se estiver diferente, a Lambda nao conseguira encontrar o assembly."
