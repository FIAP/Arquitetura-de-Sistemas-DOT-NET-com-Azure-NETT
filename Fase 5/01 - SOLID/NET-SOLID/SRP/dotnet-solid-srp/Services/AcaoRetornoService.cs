public class AcaoRetornoService
{
    public double CalcularRetornoAnual(Acao acao)
    {
        return acao.Quantidade * acao.DividendoAnual;
    }
}