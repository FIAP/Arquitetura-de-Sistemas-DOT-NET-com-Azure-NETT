public interface IInvestimentoService<T> where T : Investimento
{
    double CalcularRetornoAnual(T investimento);
}