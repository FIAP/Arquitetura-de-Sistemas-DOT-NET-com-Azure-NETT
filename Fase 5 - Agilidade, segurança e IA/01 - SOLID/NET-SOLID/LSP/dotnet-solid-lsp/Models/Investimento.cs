public abstract class Investimento
{
    public string Nome { get; set; }
    public double ValorInvestido { get; set; }

    protected Investimento(string nome, double valorInvestido)
    {
        Nome = nome;
        ValorInvestido = valorInvestido;
    }

    public abstract double CalcularRetornoAnual();
    public abstract double CalcularDepreciacao();


    public string Descrever()
    {
        return $"Investimento: {Nome}, Valor Investido: R$ {ValorInvestido}";
    }
}