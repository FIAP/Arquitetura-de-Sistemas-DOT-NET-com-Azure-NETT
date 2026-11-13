namespace Producer.Models;

public class PedidoMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string NomeCliente { get; set; } = string.Empty;
    
    public string EmailCliente { get; set; } = string.Empty;
    
    public List<ItemPedido> Itens { get; set; } = new();
    
    public decimal ValorTotal { get; set; }
    
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    
    public string Status { get; set; } = "Pendente";
    
    public override string ToString()
    {
        return $"Pedido {Id}: {NomeCliente} ({EmailCliente}) - Total: R$ {ValorTotal:F2} - {Itens.Count} itens";
    }
}

public class ItemPedido
{
    public string Nome { get; set; } = string.Empty;
    
    public int Quantidade { get; set; }
    
    public decimal PrecoUnitario { get; set; }
    
    public decimal Subtotal => Quantidade * PrecoUnitario;
}
