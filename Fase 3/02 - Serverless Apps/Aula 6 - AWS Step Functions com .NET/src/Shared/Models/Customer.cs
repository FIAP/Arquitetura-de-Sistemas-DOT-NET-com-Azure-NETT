using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class Customer
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("document")]
        public string Document { get; set; } = string.Empty; // CPF ou CNPJ

        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; } = string.Empty; // CPF ou CNPJ

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "ACTIVE";
    }

    public class CustomerRegistrationRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("document")]
        public string Document { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;
    }

    public class ValidationResult
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new List<string>();

        [JsonPropertyName("customerData")]
        public CustomerRegistrationRequest? CustomerData { get; set; }

        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; } = string.Empty;
    }

    public class CheckCustomerResult
    {
        [JsonPropertyName("exists")]
        public bool Exists { get; set; }

        [JsonPropertyName("customerId")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("validationResult")]
        public ValidationResult? ValidationResult { get; set; }
    }

    public class CreateCustomerResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [JsonPropertyName("customer")]
        public Customer? Customer { get; set; }

        [JsonPropertyName("checkResult")]
        public CheckCustomerResult? CheckResult { get; set; }
    }

    public class WelcomeEmailResult
    {
        [JsonPropertyName("sent")]
        public bool Sent { get; set; }

        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("createResult")]
        public CreateCustomerResult? CreateResult { get; set; }
    }
}
