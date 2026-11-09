public class AcaoService : IInvestimentoService<Acao>
{
    public double CalcularRetornoAnual(Acao acao)
    {
        return acao.Quantidade * acao.DividendoAnual;
    }
  
}