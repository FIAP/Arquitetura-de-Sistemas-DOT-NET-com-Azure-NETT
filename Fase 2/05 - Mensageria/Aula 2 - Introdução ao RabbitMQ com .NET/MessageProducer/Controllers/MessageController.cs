using Microsoft.AspNetCore.Mvc;
using MessageProducer.Models;
using MessageProducer.Services;

namespace MessageProducer.Controllers
{
    public class MessageController : Controller
    {
        private readonly IRabbitMQService _rabbitMQService;

        public MessageController(IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        public IActionResult Index()
        {
            return View(new MessageModel());
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(MessageModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Criar mensagem com timestamp
                var messageWithTimestamp = $"[{model.SentAt:yyyy-MM-dd HH:mm:ss}] {model.Message}";
                
                await _rabbitMQService.SendMessageAsync(messageWithTimestamp);
                
                TempData["Success"] = $"Mensagem enviada com sucesso às {model.SentAt:HH:mm:ss}!";
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao enviar mensagem: {ex.Message}");
                return View("Index", model);
            }
        }
    }
}
