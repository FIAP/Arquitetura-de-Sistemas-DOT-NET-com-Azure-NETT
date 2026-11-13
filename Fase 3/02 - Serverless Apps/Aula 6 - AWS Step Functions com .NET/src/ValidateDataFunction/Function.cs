using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Shared.Models;
using Shared.Utils;
using System.Text.RegularExpressions;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace ValidateDataFunction
{
    public class Function
    {
        private readonly Regex emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        private readonly Regex phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$|^\(\d{2}\)\s?\d{4,5}-?\d{4}$|^\d{10,11}$");

        public ValidationResult FunctionHandler(CustomerRegistrationRequest request, ILambdaContext context)
        {
            context.Logger.LogInformation($"Iniciando validação para: {request.Name}");

            var result = new ValidationResult
            {
                CustomerData = request
            };

            var errors = new List<string>();

            // Validar nome
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 2)
            {
                errors.Add("Nome deve ter pelo menos 2 caracteres");
            }

            if (request.Name?.Length > 100)
            {
                errors.Add("Nome deve ter no máximo 100 caracteres");
            }

            // Validar email
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                errors.Add("Email é obrigatório");
            }
            else if (!emailRegex.IsMatch(request.Email))
            {
                errors.Add("Email deve ter um formato válido");
            }

            // Validar telefone
            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                errors.Add("Telefone é obrigatório");
            }
            else if (!phoneRegex.IsMatch(request.Phone.Replace(" ", "").Replace("-", "")))
            {
                errors.Add("Telefone deve ter um formato válido");
            }

            // Validar documento (CPF ou CNPJ)
            if (string.IsNullOrWhiteSpace(request.Document))
            {
                errors.Add("CPF/CNPJ é obrigatório");
            }
            else
            {
                var documentType = DocumentValidator.GetDocumentType(request.Document);
                result.DocumentType = documentType;

                if (documentType == "INVALID")
                {
                    errors.Add("Documento deve ter 11 dígitos (CPF) ou 14 dígitos (CNPJ)");
                }
                else if (documentType == "CPF" && !DocumentValidator.IsValidCpf(request.Document))
                {
                    errors.Add("CPF inválido");
                }
                else if (documentType == "CNPJ" && !DocumentValidator.IsValidCnpj(request.Document))
                {
                    errors.Add("CNPJ inválido");
                }
            }

            result.Errors = errors;
            result.IsValid = !errors.Any();

            context.Logger.LogInformation($"Validação concluída. Válido: {result.IsValid}. Erros: {string.Join(", ", errors)}");

            return result;
        }
    }
}
