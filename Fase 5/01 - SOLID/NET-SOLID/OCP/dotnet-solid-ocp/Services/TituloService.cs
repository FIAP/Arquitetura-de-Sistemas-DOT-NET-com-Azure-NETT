public class TituloService : IInvestimentoService<Titulo>
{
    public double CalcularRetornoAnual(Titulo titulo)
    {
        return titulo.CalcularRetornoAnual();
    }
    
}