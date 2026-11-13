public class Imovel : Investimento
{
    public double AluguelMensal { get; set; }

    public Imovel(string nome, double valorInvestido, double aluguelMensal)
        : base(nome, valorInvestido)
    {
        AluguelMensal = aluguelMensal;
    }

    public override double CalcularRetornoAnual()
    {
        return AluguelMensal * 12;
    }

    public override string Descrever()
    {
        return $"{base.Descrever()}, Aluguel Mensal: R$ {AluguelMensal}";
    }
}