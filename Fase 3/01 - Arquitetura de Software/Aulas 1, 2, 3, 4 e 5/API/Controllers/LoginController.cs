using Application.UseCases;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(IAuthenticationUseCase useCase) : ControllerBase
    {
        private readonly IAuthenticationUseCase _useCase = useCase;


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
           
            if (_useCase.Authenticate(request.Email, request.Password))
            {
                return Ok(new { Message = "Autenticação bem-sucedida!" });
            }

            return Unauthorized(new { Message = "Falha na autenticação!" });
        }

    }
}
