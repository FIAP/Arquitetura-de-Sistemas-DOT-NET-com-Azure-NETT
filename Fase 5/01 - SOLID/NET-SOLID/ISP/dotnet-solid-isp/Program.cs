var acao = new Acao("Empresa X", 5000, 100, 50, 5);
var titulo = new Titulo("Título Y", 10000, 7);
var imovel = new Imovel("Apartamento Z", 300000, 1500);

void ImprimirRetornoAnual(Investimento investimento)
{
    Console.WriteLine(investimento.Descrever());
    Console.WriteLine($"Retorno anual: R$ {investimento.CalcularRetornoAnual()}");
}

void ImprimirDepreciacao(IDepreciacao investimento)
{
    Console.WriteLine($"Retorno depois da depreciacao: R$ {investimento.CalcularDepreciacao()}");
}

ImprimirRetornoAnual(acao);
ImprimirRetornoAnual(titulo);
ImprimirRetornoAnual(imovel);
ImprimirDepreciacao(imovel);
