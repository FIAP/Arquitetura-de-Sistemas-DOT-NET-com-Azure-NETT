namespace Dominio.Entidades;

public class Canal
{
    public Guid CanalId { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public bool Privado { get; private set; }
    public List<Usuario> Membros { get; private set; }

    public Canal(string nome, string descricao, bool privado)
    {
        CanalId = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Privado = privado;
        Membros = new List<Usuario>();
    }

    public void AdicionarMembro(Usuario usuario)
    {
        Membros.Add(usuario);
    }

    public void RemoverMembro(Usuario usuario)
    {
        Membros.Remove(usuario);
    }
}
