using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("=== MESSAGE CONSUMER ===");
Console.WriteLine("Conectando ao RabbitMQ...");

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "admin",
    Password = "admin123"
};

try
{
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    string queueName = "mensagens";

    // Declara a fila (criar se não existir)
    await channel.QueueDeclareAsync(
        queue: queueName,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null);

    Console.WriteLine($"Aguardando mensagens da fila '{queueName}'...");
    Console.WriteLine("Pressione CTRL+C para sair.");

    var consumer = new AsyncEventingBasicConsumer(channel);
    
    consumer.ReceivedAsync += async (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Mensagem recebida: {message}");
        
        // Simula processamento
        await Task.Delay(1000);
        
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Mensagem processada com sucesso!");
    };

    await channel.BasicConsumeAsync(
        queue: queueName,
        autoAck: true,
        consumer: consumer);

    // Mantém o consumer rodando
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
    Console.WriteLine("Certifique-se de que o RabbitMQ está rodando (execute: ./rabbitmq.sh start)");
}
