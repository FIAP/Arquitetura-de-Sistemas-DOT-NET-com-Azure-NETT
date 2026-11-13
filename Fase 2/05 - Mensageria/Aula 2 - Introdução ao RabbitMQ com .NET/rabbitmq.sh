#!/bin/bash

# Script para gerenciar RabbitMQ local

case "$1" in
    start)
        echo "Iniciando RabbitMQ..."
        docker-compose up -d
        echo "RabbitMQ iniciado! Acesse http://localhost:15672 (admin/admin123)"
        ;;
    stop)
        echo "Parando RabbitMQ..."
        docker-compose down
        ;;
    restart)
        echo "Reiniciando RabbitMQ..."
        docker-compose restart
        ;;
    logs)
        echo "Visualizando logs do RabbitMQ..."
        docker-compose logs -f rabbitmq
        ;;
    status)
        echo "Status do RabbitMQ:"
        docker-compose ps
        ;;
    clean)
        echo "Limpando dados do RabbitMQ..."
        docker-compose down -v
        ;;
    shell)
        echo "Acessando shell do RabbitMQ..."
        docker-compose exec rabbitmq bash
        ;;
    queues)
        echo "Listando filas:"
        docker-compose exec rabbitmq rabbitmqctl list_queues
        ;;
    exchanges)
        echo "Listando exchanges:"
        docker-compose exec rabbitmq rabbitmqctl list_exchanges
        ;;
    *)
        echo "Uso: $0 {start|stop|restart|logs|status|clean|shell|queues|exchanges}"
        echo ""
        echo "Comandos disponíveis:"
        echo "  start      - Inicia o RabbitMQ"
        echo "  stop       - Para o RabbitMQ"
        echo "  restart    - Reinicia o RabbitMQ"
        echo "  logs       - Mostra os logs"
        echo "  status     - Mostra o status"
        echo "  clean      - Para e remove todos os dados"
        echo "  shell      - Acessa o shell do container"
        echo "  queues     - Lista as filas"
        echo "  exchanges  - Lista os exchanges"
        exit 1
        ;;
esac
