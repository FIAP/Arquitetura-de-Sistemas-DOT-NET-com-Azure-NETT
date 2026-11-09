public class Acao : Investimento
{
    public int Quantidade { get; set; }
    public double PrecoPorAcao { get; set; }
    public double DividendoAnual { get; set; }

    public Acao(string nome, double valorInvestido, int quantidade, double precoPorAcao, double dividendoAnual)
        : base(nome, valorInvestido)
    {
        Quantidade = quantidade;
        PrecoPorAcao = precoPorAcao;
        DividendoAnual = dividendoAnual;
    }   

    public override double CalcularRetornoAnual()
    {
        return Quantidade * DividendoAnual;
    }
  
}