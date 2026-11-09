### Criando estrutura

```shell
CREATE TABLE IF NOT EXISTS quotes (
    id SERIAL PRIMARY KEY,
    symbol VARCHAR(10) NOT NULL,
    exchange VARCHAR(20) NOT NULL,
    regularMarketPrice NUMERIC(10, 2) NOT NULL
);
```


### Criando uma Carga Inicial de Dados

Para criar uma carga inicial de dados na tabela "quotes", você pode executar o seguinte comando SQL após o PostgreSQL estar em execução:

```shell
INSERT INTO quotes (symbol, exchange, regularMarketPrice)
VALUES
    ('AAPL', 'NASDAQ', 150.25),
    ('GOOGL', 'NASDAQ', 2700.50),
    ('MSFT', 'NASDAQ', 300.75),
    ('AMZN', 'NASDAQ', 3500.00),
    ('TSLA', 'NASDAQ', 750.60);
```