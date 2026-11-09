public class AcaoDescricaoService
{
    public string Descrever(Acao acao)
    {
        return $"Ação: {acao.Nome}, Quantidade: {acao.Quantidade}, Preço por Ação: R$ {acao.PrecoPorAcao}, Dividendo Anual: R$ {acao.DividendoAnual}";
    }
}