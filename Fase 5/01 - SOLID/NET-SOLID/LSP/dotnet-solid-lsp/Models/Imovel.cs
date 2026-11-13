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

    public override  double CalcularDepreciacao()
    {
        return ValorInvestido * 0.01;
    }
}