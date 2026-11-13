# 🚀 AWS Lambda - Casos de Uso e Diagramas de Sequência

Esta documentação apresenta **5 casos de uso práticos** e referenciais para AWS Lambda no contexto de aplicações financeiras e corporativas.

## 📋 Índice dos Casos

1. **[API REST para Consulta de Fundos](./caso-1-api-rest.md)**
   - Consulta de cotações em tempo real
   - Cache inteligente com DynamoDB
   - Latência baixa (~200ms)

2. **[Processamento Assíncrono de Pedidos](./caso-2-processamento-assincrono.md)**
   - Processamento em lote via SQS
   - Alta disponibilidade e retry automático
   - Notificações via SNS

3. **[Job Agendado - Sincronização Diária](./caso-3-job-agendado.md)**
   - Coleta automática de dados de mercado
   - Execução agendada com CloudWatch Events
   - Backup automático em S3

4. **[Processamento de Arquivo CSV](./caso-4-processamento-arquivo.md)**
   - Upload e processamento automático
   - Validação e importação em lote
   - Relatórios de resultado via email

5. **[Microservices - Análise de Portfólio](./caso-5-microservices.md)**
   - Arquitetura de microserviços
   - Análise de risco com Machine Learning
   - Cache distribuído com Redis

## 🎯 Benefícios do AWS Lambda

- **💰 Custo**: Pay-per-use (sem servidores ociosos)
- **📈 Escalabilidade**: Auto-scaling automático
- **🔧 Manutenção**: Zero infraestrutura para gerenciar
- **⚡ Performance**: Cold start ~200ms, warm ~10ms
- **🛡️ Confiabilidade**: 99.95% SLA da AWS

## 📊 Comparativo de Casos

| Caso | Latência | Custo/mês | Escalabilidade | Complexidade |
|------|----------|-----------|----------------|--------------|
| API REST | ~200ms | $5-20 | Alta | Baixa |
| Async Processing | ~1-5s | $10-50 | Muito Alta | Média |
| Job Agendado | N/A | $0.02-2 | Baixa | Baixa |
| CSV Processing | ~2-10min | $1-10 | Média | Média |
| Microservices | ~500ms | $20-100 | Muito Alta | Alta |

## 🔧 Tecnologias Utilizadas

- **AWS Lambda** - Computação serverless
- **API Gateway** - Endpoints REST/HTTP
- **DynamoDB** - Database NoSQL
- **S3** - Storage de arquivos
- **SQS** - Filas de mensagens
- **SNS** - Notificações push
- **CloudWatch** - Monitoramento e logs
- **SES** - Envio de emails
- **Redis** - Cache distribuído
