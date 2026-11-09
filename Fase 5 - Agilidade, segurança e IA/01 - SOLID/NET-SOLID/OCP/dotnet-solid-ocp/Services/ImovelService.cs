public class ImovelService : IInvestimentoService<Imovel>
{
    public double CalcularRetornoAnual(Imovel imovel)
    {
        return imovel.CalcularRetornoAnual();
    }
   
}