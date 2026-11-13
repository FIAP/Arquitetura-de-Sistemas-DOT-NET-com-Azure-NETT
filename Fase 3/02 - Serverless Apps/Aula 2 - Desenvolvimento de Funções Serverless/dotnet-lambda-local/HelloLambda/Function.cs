using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloLambda;

public class Function
{
	public record Request(string? Name);
	public record Response(string Message, DateTime TimestampUtc);

	public Response FunctionHandler(Request request, ILambdaContext context)
	{
		var name = string.IsNullOrWhiteSpace(request?.Name) ? "mundo" : request!.Name!.Trim();
		var message = $"Olá, {name}! Sua função .NET 8 na AWS Lambda está rodando no LocalStack.";
		context.Logger.LogInformation($"RequestId={context.AwsRequestId} Name={name}");
		return new Response(message, DateTime.UtcNow);
	}
}


