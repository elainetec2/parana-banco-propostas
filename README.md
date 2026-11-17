# Paraná Banco — Microsserviço de Propostas de Crédito

Microsserviço responsável pela análise e geração de propostas de crédito, desenvolvido em **.NET 10**, utilizando **RabbitMQ, MassTransit, Entity Framework Core e SQL Server**.

Este projeto integra uma solução de três microsserviços independentes: Clientes, Propostas e Cartões.

## 1. Responsabilidades

- Consumir o evento `ClienteCadastrado` publicado pelo microsserviço de Clientes.
- Calcular um score de crédito simulado para o cliente.
- Aplicar as regras de aprovação e definição de limites.
- Persistir a proposta no banco de dados próprio.
- Publicar o evento `PropostaGerada` para o microsserviço de Cartões.
- Disponibilizar um endpoint REST para consulta de propostas.

## 2. Regras de negócio

| Score | Situação | Limite por cartão | Quantidade máxima de cartões |
|---|---|---|---|
| 0–100 | Negado | — | 0 |
| 101–500 | Aprovado | R$ 1.000 | 1 |
| 501–1000 | Aprovado | R$ 5.000 | 2 |

A regra de crédito é implementada em `RegraDeCredito`, na camada Domain.

### Cálculo do score

Como o desafio não fornece uma fórmula para determinar o score a partir dos dados cadastrais, foi implementada uma **simulação determinística**, utilizando o CPF normalizado.

O algoritmo:

1. Remove caracteres não numéricos do CPF.
2. Calcula seu hash SHA-256.
3. Converte os primeiros quatro bytes do hash em um inteiro sem sinal.
4. Aplica módulo 1001 para obter um score entre 0 e 1000.

Essa abordagem garante que o mesmo CPF produza o mesmo score durante os testes.

**Importante:** esse cálculo não representa um modelo real de análise de risco de crédito e não deve ser utilizado para decisões financeiras reais.

## 3. Arquitetura

O projeto está organizado nas seguintes camadas:

- **Domain:** entidade `Proposta`, `RegraDeCredito` e `CalculadoraScore`.
- **Application:** contratos dos eventos `ClienteCadastrado` e `PropostaGerada`.
- **Infrastructure:** consumidor MassTransit, Entity Framework Core, migrations e configuração de dependências.
- **API:** endpoint REST para consulta de propostas.
- **UnitTests:** projeto destinado aos testes automatizados.

O microsserviço possui banco próprio e se comunica com os demais por eventos, sem acesso direto às suas tabelas.

## 4. Tecnologias

- .NET 10 / ASP.NET Core
- C#
- Entity Framework Core 10
- SQL Server LocalDB
- RabbitMQ 4
- MassTransit 8.5.7
- xUnit
- Docker Desktop para execução do RabbitMQ

## 5. Pré-requisitos

Para executar localmente no Windows:

- .NET SDK 10
- SQL Server LocalDB
- Docker Desktop
- RabbitMQ em execução
- Ferramenta `dotnet-ef` compatível com EF Core 10

O RabbitMQ pode ser iniciado utilizando o `docker-compose.yml` disponível no repositório de Clientes:

```powershell
docker compose up -d
```

Execute esse comando na pasta que contém o arquivo `docker-compose.yml`.

O painel de gerenciamento do RabbitMQ fica disponível em `http://localhost:15672`, utilizando `guest` / `guest` no ambiente local.

## 6. Configuração

A conexão com o SQL Server LocalDB é configurada em:

`ParanaBanco.Propostas.API/appsettings.Development.json`

Exemplo:

```json
{
  "ConnectionStrings": {
    "PropostasDb": "Server=(localdb)\\MSSQLLocalDB;Database=ParanaBancoPropostas;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

O RabbitMQ utiliza `localhost:5672`, virtual host `/` e credenciais locais `guest` / `guest`.

Essas configurações são destinadas ao desenvolvimento. Em ambientes compartilhados ou de produção, as credenciais devem ser externalizadas.

## 7. Como executar

Na raiz do repositório:

**Restaurar dependências:**

```powershell
dotnet restore ParanaBanco.Propostas.slnx
```

**Aplicar migrations:**

```powershell
dotnet ef database update --project ParanaBanco.Propostas.Infrastructure --startup-project ParanaBanco.Propostas.API --context PropostasDbContext
```

**Iniciar a API:**

```powershell
dotnet run --project ParanaBanco.Propostas.API
```

No perfil HTTP utilizado nos testes locais, a API fica disponível em:

`http://localhost:5096`

Para receber novos eventos, mantenha o RabbitMQ em execução e inicie também o microsserviço de Clientes.

## 8. Endpoint REST

### GET /api/v1/propostas/{clienteId}

Retorna a proposta gerada para o cliente informado.

Exemplo:

```http
GET http://localhost:5096/api/v1/propostas/bec66c0e-d2ba-4673-a114-f97b9cb0eec4
```

Resposta de exemplo:

```json
{
  "id": "5191744d-ad71-4c4c-89e1-9fdd331fa234",
  "clienteId": "bec66c0e-d2ba-4673-a114-f97b9cb0eec4",
  "score": 567,
  "aprovada": true,
  "limitePorCartao": 5000.00,
  "quantidadeMaximaCartoes": 2,
  "criadaEm": "2026-11-17T15:15:49.2173863"
}
```

Quando a proposta não é encontrada, o endpoint retorna HTTP 404.

## 9. Mensageria e resiliência

### Evento consumido

`ParanaBanco.Clientes.Application.Events.ClienteCadastrado`

O contrato é definido localmente com o mesmo nome completo e estrutura do evento publicado por Clientes.

Fila de consumo:

`propostas-cliente-cadastrado`

### Evento publicado

`ParanaBanco.Propostas.Application.Events.PropostaGerada`

O evento contém o identificador da proposta e do cliente, o score, o resultado da aprovação, o limite por cartão, a quantidade máxima de cartões e a data de ocorrência.

### Tratamento de falhas

O MassTransit é utilizado para consumir mensagens do RabbitMQ.

A infraestrutura utiliza mecanismos de retry e Consumer Outbox para apoiar o processamento confiável e reduzir os efeitos de reentregas.

Mensagens que esgotam as tentativas de processamento podem ser encaminhadas para a fila de erro do endpoint, permitindo investigação e tratamento posterior.

A persistência de propostas utiliza um índice único em `ClienteId`, reforçando a regra de uma proposta por cliente.

## 10. Testes

Para executar os testes disponíveis:

```powershell
dotnet test ParanaBanco.Propostas.slnx
```

As regras de score e aprovação foram verificadas durante a integração local dos microsserviços. A cobertura automatizada pode ser ampliada para incluir mais cenários de consumo, falhas e idempotência.

## 11. Fluxo de integração

1. Clientes cadastra um cliente e publica `ClienteCadastrado`.
2. Propostas recebe o evento pelo RabbitMQ.
3. O consumidor calcula o score e cria a proposta.
4. A proposta é persistida no SQL Server.
5. O evento `PropostaGerada` é publicado.
6. Cartões recebe o evento e realiza a emissão conforme as regras de negócio.

### Validação local realizada

Em 17/11/2026, foi validado o fluxo completo para um cliente com score **112**, que recebeu uma proposta aprovada e um cartão de R$ 1.000.

Também foi validada uma proposta com score **567**, aprovada com limite de R$ 5.000 por cartão e quantidade máxima de dois cartões.

## 12. Decisões técnicas e limitações

- Cada microsserviço mantém seu próprio banco de dados.
- A comunicação entre serviços utiliza eventos assíncronos.
- O score é uma simulação técnica, não uma avaliação financeira real.
- O CPF é utilizado apenas como entrada determinística para o cálculo simulado.
- A arquitetura separa regras de negócio, persistência e transporte.
- O ambiente de desenvolvimento utiliza SQL Server LocalDB no Windows e RabbitMQ no Docker.
- Os mecanismos de retry, Outbox e idempotência podem receber testes adicionais de falhas e concorrência.

## 13. Repositórios relacionados

- [Cadastro de Clientes](https://github.com/elainetec2/parana-banco-clientes)
- [Cartões de Crédito](https://github.com/elainetec2/parana-banco-cartoes)

**Repositório atual:** [Paraná Banco — Propostas](https://github.com/elainetec2/parana-banco-propostas)
