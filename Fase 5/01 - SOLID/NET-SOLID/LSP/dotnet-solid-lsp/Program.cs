Investimento acao = new Acao("Empresa X", 5000, 100, 50, 5);
Investimento titulo = new Titulo("Título Y", 10000, 7);
Investimento imovel = new Imovel("Apartamento Z", 300000, 1500);

void ImprimirRetornoAnual(Investimento investimento)
{
    Console.WriteLine(investimento.Descrever());
    Console.WriteLine($"Retorno anual: R$ {investimento.CalcularRetornoAnual()}");
}

ImprimirRetornoAnual(acao);
ImprimirRetornoAnual(titulo);
ImprimirRetornoAnual(imovel);
Console.WriteLine($"Depreciação anual: R$ {imovel.CalcularDepreciacao()}");