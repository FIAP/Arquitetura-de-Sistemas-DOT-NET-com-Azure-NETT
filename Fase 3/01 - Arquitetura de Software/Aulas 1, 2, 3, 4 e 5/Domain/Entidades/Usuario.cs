using Domain.Entities.Base;
namespace Dominio.Entidades;

public class Usuario
{
    public Guid UsuarioId { get; private set; }
    public string NomeUsuario { get; private set; }
    public string Email { get; private set; }
    public string HashSenha { get; private set; }
    public string UrlFotoPerfil { get; private set; }

    public Usuario(string nomeUsuario, string email, string hashSenha)
    {
        UsuarioId = Guid.NewGuid();
        NomeUsuario = nomeUsuario;
        Email = email;
        HashSenha = hashSenha;
        ValidateEntity();
    }

    public void AtualizarPerfil(string novoNomeUsuario, string novoEmail, string novaUrlFotoPerfil)
    {
        NomeUsuario = novoNomeUsuario;
        Email = novoEmail;
        UrlFotoPerfil = novaUrlFotoPerfil;

    }

    public void AlterarSenha(string novaHashSenha)
    {
        HashSenha = novaHashSenha;
    }

    public void ValidateEntity()
    {
        AssertionConcern.AssertArgumentNotEmpty(NomeUsuario, "O nome não pode estar vazio!");
    }
}
