using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Shared.Models;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace SendWelcomeEmailFunction
{
    public class Function
    {
        private readonly AmazonSimpleNotificationServiceClient _snsClient;
        private readonly string _topicArn;

        public Function()
        {
            var config = new AmazonSimpleNotificationServiceConfig();
            
            // Configuração para LocalStack
            var localStackEndpoint = Environment.GetEnvironmentVariable("LOCALSTACK_ENDPOINT");
            if (!string.IsNullOrEmpty(localStackEndpoint))
            {
                config.ServiceURL = localStackEndpoint;
                config.UseHttp = true;
            }

            _snsClient = new AmazonSimpleNotificationServiceClient(config);
            _topicArn = Environment.GetEnvironmentVariable("WELCOME_EMAIL_TOPIC_ARN") ?? "arn:aws:sns:us-east-1:000000000000:welcome-email";
        }

        public async Task<WelcomeEmailResult> FunctionHandler(CreateCustomerResult createResult, ILambdaContext context)
        {
            context.Logger.LogInformation($"Iniciando envio de email de boas-vindas");

            var result = new WelcomeEmailResult
            {
                CreateResult = createResult
            };

            if (!createResult.Success || createResult.Customer == null)
            {
                context.Logger.LogInformation("Cliente não foi criado com sucesso, pulando envio de email");
                result.Sent = false;
                return result;
            }

            try
            {
                var customer = createResult.Customer;
                var emailMessage = CreateWelcomeEmailMessage(customer);

                var publishRequest = new PublishRequest
                {
                    TopicArn = _topicArn,
                    Message = JsonSerializer.Serialize(emailMessage),
                    Subject = "Novo Cliente Cadastrado - Email de Boas-vindas"
                };

                var response = await _snsClient.PublishAsync(publishRequest);

                result.Sent = true;
                result.MessageId = response.MessageId;

                context.Logger.LogInformation($"Email de boas-vindas enviado com sucesso. MessageId: {response.MessageId}");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Erro ao enviar email de boas-vindas: {ex.Message}");
                result.Sent = false;
            }

            return result;
        }

        private EmailMessage CreateWelcomeEmailMessage(Customer customer)
        {
            var welcomeText = customer.DocumentType switch
            {
                "CPF" => $"Olá, {customer.Name}!",
                "CNPJ" => $"Olá, {customer.Name}!",
                _ => $"Olá, {customer.Name}!"
            };

            return new EmailMessage
            {
                To = customer.Email,
                Subject = "Bem-vindo(a)! Seu cadastro foi realizado com sucesso",
                Body = $@"{welcomeText}

Seja muito bem-vindo(a) à nossa plataforma!

Seu cadastro foi realizado com sucesso com os seguintes dados:
- Nome: {customer.Name}
- Email: {customer.Email}
- Telefone: {customer.Phone}
- Documento: {customer.Document} ({customer.DocumentType})
- ID do Cliente: {customer.Id}
- Data de Cadastro: {customer.CreatedAt:dd/MM/yyyy HH:mm:ss}

Agradecemos por escolher nossos serviços!

Atenciosamente,
Equipe de Cadastro"
            };
        }
    }

    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
