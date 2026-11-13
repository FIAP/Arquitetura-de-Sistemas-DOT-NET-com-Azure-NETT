using System.Linq;
using System.Text.RegularExpressions;

namespace Shared.Utils
{
    public static class DocumentValidator
    {
        public static bool IsValidCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove não numéricos
            cpf = Regex.Replace(cpf, @"[^\d]", "");

            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais
            if (cpf.Distinct().Count() == 1)
                return false;

            // Calcula os dígitos verificadores
            var soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * (10 - i);

            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;

            if (int.Parse(cpf[9].ToString()) != digito1)
                return false;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * (11 - i);

            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;

            return int.Parse(cpf[10].ToString()) == digito2;
        }

        public static bool IsValidCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            // Remove não numéricos
            cnpj = Regex.Replace(cnpj, @"[^\d]", "");

            if (cnpj.Length != 14)
                return false;

            // Verifica se todos os dígitos são iguais
            if (cnpj.Distinct().Count() == 1)
                return false;

            // Calcula primeiro dígito verificador
            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(cnpj[i].ToString()) * multiplicador1[i];

            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;

            if (int.Parse(cnpj[12].ToString()) != digito1)
                return false;

            // Calcula segundo dígito verificador
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(cnpj[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;

            return int.Parse(cnpj[13].ToString()) == digito2;
        }

        public static string GetDocumentType(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                return "INVALID";

            var cleanDocument = Regex.Replace(document, @"[^\d]", "");

            return cleanDocument.Length switch
            {
                11 => "CPF",
                14 => "CNPJ",
                _ => "INVALID"
            };
        }

        public static string CleanDocument(string document)
        {
            return Regex.Replace(document ?? "", @"[^\d]", "");
        }
    }
}
