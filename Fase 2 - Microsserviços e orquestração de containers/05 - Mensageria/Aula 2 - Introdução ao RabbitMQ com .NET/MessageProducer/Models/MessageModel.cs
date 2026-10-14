using System.ComponentModel.DataAnnotations;

namespace MessageProducer.Models
{
    public class MessageModel
    {
        [Required(ErrorMessage = "A mensagem é obrigatória")]
        [StringLength(500, ErrorMessage = "A mensagem deve ter no máximo 500 caracteres")]
        public string Message { get; set; } = string.Empty;
        
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
