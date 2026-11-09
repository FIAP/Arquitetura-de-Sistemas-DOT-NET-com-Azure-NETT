public class Titulo : Investimento
{
    public double TaxaDeJurosAnual { get; set; }

    public Titulo(string nome, double valorInvestido, double taxaDeJurosAnual)
        : base(nome, valorInvestido)
    {
        TaxaDeJurosAnual = taxaDeJurosAnual;
    }

    public override double CalcularRetornoAnual()
    {
        return ValorInvestido * (TaxaDeJurosAnual / 100);
    }

public override double CalcularDepreciacao()
 {
       throw new NotImplementedException();
 }

   

}