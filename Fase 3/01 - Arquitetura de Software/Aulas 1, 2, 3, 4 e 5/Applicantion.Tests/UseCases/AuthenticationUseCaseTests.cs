using Application.UseCases;
using Dominio.Entidades;
using Application.Repository;
using Moq;

namespace Applicantion.Tests
{
    public class AuthenticationUseCaseTests
    {
        private readonly Mock<IAuthenticationRepository> _mockAuthRepository;
        private readonly AuthenticationUseCase _useCase;

        public AuthenticationUseCaseTests()
        {
            _mockAuthRepository = new Mock<IAuthenticationRepository>();
            _useCase = new AuthenticationUseCase(_mockAuthRepository.Object);
        }

        [Fact]
        public void Authenticate_ShouldReturnTrue_WhenCredentialsAreValid()
        {
            // Configurar o mock para retornar true para credenciais válidas
            _mockAuthRepository.Setup(x => x.Authenticate("tadriano@teste.com", "102030")).Returns(true);

            // Executar
            bool result = _useCase.Authenticate("tadriano@teste.com", "102030");

            // Verificar
            Assert.True(result);
            _mockAuthRepository.Verify(x => x.Authenticate("tadriano@teste.com", "102030"), Times.Once);
        }

        [Fact]
        public void Authenticate_ShouldReturnFalse_WhenCredentialsAreInvalid()
        {
            // Configurar o mock para retornar false para credenciais inválidas
            _mockAuthRepository.Setup(x => x.Authenticate("intadriano@teste.com", "in10203040")).Returns(false);

            // Executar
            bool result = _useCase.Authenticate("intadriano@teste.com", "in10203040");

            // Verificar
            Assert.False(result);
            _mockAuthRepository.Verify(x => x.Authenticate("intadriano@teste.com", "in10203040"), Times.Once);
        }

    }
}