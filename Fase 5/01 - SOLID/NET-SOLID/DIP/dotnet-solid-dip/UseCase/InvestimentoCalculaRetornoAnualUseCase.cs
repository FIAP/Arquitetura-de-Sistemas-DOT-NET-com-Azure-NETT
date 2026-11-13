public class InvestimentoCalculaRetornoAnualUseCase
{
    public double CalcularRetornoAnualAcao(Investimento  investimento)
    {
        return investimento.CalcularRetornoAnual();
    }
 

    public double CalcularDepreciacao(IDepreciacao depreciacao)
    {
        return depreciacao.CalcularDepreciacao();
    }
}