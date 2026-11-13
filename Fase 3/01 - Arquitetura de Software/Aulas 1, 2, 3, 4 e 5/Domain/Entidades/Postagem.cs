namespace Dominio.Entidades;

public class Postagem
{
    public Guid PostagemId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Conteudo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<Comentario> Comentarios { get; private set; }

    public Postagem(Guid usuarioId, string conteudo)
    {
        PostagemId = Guid.NewGuid();
        UsuarioId = usuarioId;
        Conteudo = conteudo;
        DataCriacao = DateTime.UtcNow;
        Comentarios = new List<Comentario>();
    }

    public void AdicionarComentario(Comentario comentario)
    {
        Comentarios.Add(comentario);
    }
}

public class Comentario
{
    public Guid ComentarioId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Texto { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public Comentario(Guid usuarioId, string texto)
    {
        ComentarioId = Guid.NewGuid();
        UsuarioId = usuarioId;
        Texto = texto;
        DataCriacao = DateTime.UtcNow;
    }
}
