export const handler = async (event) => {
    console.log('Event received:', JSON.stringify(event, null, 2));
    
    // Extrair symbol de diferentes fontes possíveis
    let symbol;
    
    // 1. Se veio direto no evento (teste Lambda console)
    if (event.symbol) {
        symbol = event.symbol;
    }
    // 2. Se veio via API Gateway query parameters (?symbol=AAPL)
    else if (event.queryStringParameters && event.queryStringParameters.symbol) {
        symbol = event.queryStringParameters.symbol;
    }
    // 3. Se veio via API Gateway path parameters (/stocks/{symbol})
    else if (event.pathParameters && event.pathParameters.symbol) {
        symbol = event.pathParameters.symbol;
    }
    // 4. Se veio no body (POST request)
    else if (event.body) {
        try {
            const body = JSON.parse(event.body);
            symbol = body.symbol;
        } catch (e) {
            console.log('Error parsing body:', e);
        }
    }

    console.log('Extracted symbol:', symbol);

    if (!symbol) {
        return {
            statusCode: 400,
            headers: {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            body: JSON.stringify({
                error: 'Missing parameter: symbol is required.',
                message: 'Please provide symbol in query parameters, path parameters, or request body',
                examples: {
                    'query': '?symbol=AAPL',
                    'path': '/stocks/AAPL',
                    'body': '{"symbol": "AAPL"}'
                }
            })
        };
    }

    // Substituir por sua chave real da Alpha Vantage
    const apiKey = 'your_alpha_vantage_api_key'; // ALTERE AQUI!
    const url = `https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=${symbol}&apikey=${apiKey}`;

    try {
        console.log('Fetching from URL:', url);
        
        const response = await fetch(url);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        
        console.log('API response received');
        
        return {
            statusCode: 200,
            headers: {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            body: JSON.stringify({
                symbol: symbol,
                data: data,
                timestamp: new Date().toISOString()
            })
        };
    } catch (error) {
        console.error('Error fetching data:', error);
        
        return {
            statusCode: 500,
            headers: {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            body: JSON.stringify({
                error: 'Error fetching data from Alpha Vantage API.',
                details: error.message,
                symbol: symbol
            })
        };
    }
};
