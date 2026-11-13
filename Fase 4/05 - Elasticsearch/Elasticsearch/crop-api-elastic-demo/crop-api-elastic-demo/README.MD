- text md

## Licença do AutoMapper

Este projeto usa o AutoMapper 15, que passou a exigir uma chave de licença. Existe uma licença
Community gratuita (veja as condições em https://automapper.io). Sem a chave o projeto funciona
normalmente, mas o AutoMapper registra um aviso no log.

Para informar a chave, use o `appsettings.json`:

```json
{
  "AutoMapper": {
    "LicenseKey": "sua-chave-aqui"
  }
}
```

ou a variável de ambiente `AutoMapper__LicenseKey`.
