# Vendinha

API para informatizar o controle de contas (dívidas "penduradas") dos clientes de uma vendinha. Permite cadastrar clientes, registrar dívidas, marcar pagamentos e consultar quem mais deve.

Desenvolvida em **C# / .NET 8** (ASP.NET Core Web API) com **Entity Framework Core** e **SQLite**.

## Requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download) (ou superior)
- Opcional: [Postman](https://www.postman.com/) ou a extensão de requisições HTTP do Visual Studio para testar a API

Não é preciso instalar banco de dados: o SQLite é um arquivo local.

## Como executar

```bash
cd Vendinha
dotnet run
```

A API sobe em **http://localhost:5039**.

Na primeira execução o arquivo `vendinha.db` é criado automaticamente na pasta `Vendinha`, junto com as tabelas. Para recomeçar do zero, pare a aplicação e apague `vendinha.db`, `vendinha.db-wal` e `vendinha.db-shm`.

Em ambiente de desenvolvimento, a descrição OpenAPI da API fica em `http://localhost:5039/openapi/v1.json`, e a interface Swagger UI em `http://localhost:5039/swagger`.

## Banco de dados

- Banco: SQLite (arquivo `vendinha.db`).
- Script de criação do schema: [`Vendinha/Scripts/schema.sql`](Vendinha/Scripts/schema.sql). Para criar o banco manualmente com ele: `sqlite3 vendinha.db < Scripts/schema.sql`.
- A própria aplicação cria o mesmo schema ao iniciar (`EnsureCreated`), então o script é opcional.

| Tabela | Campos |
|---|---|
| `Clientes` | `Id`, `NomeCompleto`, `CPF` (único), `DataNascimento`, `Email` (opcional) |
| `Dividas` | `Id`, `Valor`, `Paga`, `DataCriacao`, `DataPagamento` (opcional), `ClienteId` |

Regras garantidas também pelo banco:
- **CPF único**: índice único em `Clientes (CPF)`.
- **Uma dívida em aberto por cliente**: índice único parcial em `Dividas (ClienteId)` onde `Paga = 0`.
- Ao excluir um cliente, as dívidas dele são excluídas junto (`ON DELETE CASCADE`).

## Regras de negócio

- Nome completo, CPF e data de nascimento são obrigatórios.
- O CPF precisa ser válido (dígitos verificadores) e não pode se repetir. É guardado apenas com números: `529.982.247-25` e `52998224725` são o mesmo CPF.
- A data de nascimento não pode estar no futuro.
- O e-mail é opcional; se informado, precisa ter formato válido.
- A idade não é guardada: é calculada a partir da data de nascimento a cada resposta.
- Um cliente só pode ter **uma dívida em aberto** por vez. Para criar outra, a anterior precisa estar paga.
- `totalDividas` do cliente é a soma das suas dívidas em aberto.

## Endpoints

### Clientes

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `POST` | `/clientes` | Cadastra um cliente | 201, 400, 409 |
| `GET` | `/clientes` | Lista paginada (10 por página), de quem mais deve para quem menos deve | 200, 400 |
| `GET` | `/clientes/{id}` | Busca um cliente | 200, 404 |
| `PUT` | `/clientes/{id}` | Atualiza um cliente | 200, 400, 404, 409 |
| `DELETE` | `/clientes/{id}` | Exclui um cliente (e suas dívidas) | 204, 404 |

Parâmetros opcionais do `GET /clientes`:
- `busca`: filtra clientes cujo nome contém o texto (ex.: `/clientes?busca=maria`)
- `pagina`: número da página, começando em 1 (ex.: `/clientes?pagina=2`)

Exemplo de corpo para `POST` e `PUT`:

```json
{
  "nomeCompleto": "Maria Silva",
  "cpf": "529.982.247-25",
  "dataNascimento": "1990-05-10",
  "email": "maria@email.com"
}
```

Exemplo de resposta de `GET /clientes`:

```json
{
  "itens": [
    {
      "id": 1,
      "nomeCompleto": "Maria Silva",
      "cpf": "52998224725",
      "dataNascimento": "1990-05-10",
      "idade": 36,
      "email": "maria@email.com",
      "totalDividas": 150.50
    }
  ],
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalItens": 1,
  "totalPaginas": 1
}
```

### Dívidas

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `POST` | `/clientes/{clienteId}/dividas` | Cria uma dívida para o cliente | 201, 400, 404, 409 |
| `GET` | `/clientes/{clienteId}/dividas` | Lista as dívidas do cliente (pagas e em aberto) | 200, 404 |
| `GET` | `/dividas/{id}` | Busca uma dívida | 200, 404 |
| `PUT` | `/dividas/{id}/pagar` | Marca a dívida como paga e registra a data do pagamento | 200, 404, 409 |

Exemplo de corpo para `POST /clientes/{clienteId}/dividas`:

```json
{ "valor": 150.50 }
```

Exemplo de resposta:

```json
{
  "id": 1,
  "valor": 150.50,
  "paga": false,
  "dataCriacao": "2026-11-11T10:30:00",
  "dataPagamento": null,
  "clienteId": 1
}
```

### Erros

Os erros de regra de negócio voltam em JSON com a mensagem:

```json
{ "erro": "CPF inválido." }
```

| Código | Quando acontece |
|---|---|
| 400 | Dado inválido (nome vazio, CPF inválido, e-mail inválido, data no futuro, valor da dívida ≤ 0, página < 1) |
| 404 | Cliente ou dívida não encontrado |
| 409 | CPF já cadastrado, cliente já tem dívida em aberto, dívida já paga |

## Front-end

A API serve uma interface web simples (HTML, CSS e JavaScript puros, sem build) que consome os endpoints acima. Com a API rodando, abra **http://localhost:5039** no navegador.

- Lista de clientes com busca por nome e paginação
- Cadastro, edição e exclusão de cliente
- Dívidas do cliente: registrar, marcar como paga e ver o histórico
- Mensagens de erro vindas da API (CPF inválido, CPF repetido, dívida em aberto etc.)

O arquivo fica em [`Vendinha/wwwroot/index.html`](Vendinha/wwwroot/index.html) e é entregue por `UseDefaultFiles` e `UseStaticFiles` no `Program.cs`. Como a página e a API compartilham a mesma origem, não é preciso configurar CORS.

## Testando

O arquivo [`Vendinha/Vendinha.http`](Vendinha/Vendinha.http) traz requisições de exemplo. As mesmas rotas podem ser testadas no Postman, apontando para `http://localhost:5039`.

CPFs válidos para testes: `529.982.247-25` e `111.444.777-35`.

## Estrutura do projeto

```
Vendinha/
├── Controladores/        # Recebem as requisições HTTP e devolvem as respostas
│   ├── ClientesController.cs
│   └── DividasController.cs
├── Servicos/             # Regras de negócio e acesso aos dados
│   ├── ClienteServico.cs
│   ├── DividaServico.cs
│   └── ValidadorCpf.cs
├── Modelos/              # Entidades do banco (Cliente, Divida)
├── ObjetosTransferencia/ # DTOs: o que a API recebe e devolve
├── Dados/                # AppDbContext (Entity Framework Core)
├── wwwroot/index.html    # Front-end (página única)
├── Scripts/schema.sql    # Script de criação do banco
└── Program.cs            # Configuração da aplicação
```

## Decisões técnicas

- **DTOs separados das entidades**: a API nunca expõe diretamente o modelo do banco; campos calculados (idade, total de dívidas) existem só na resposta.
- **Ordenação e paginação em memória**: o SQLite não ordena nem soma valores `decimal` direto no banco, então a listagem de clientes calcula o total em aberto e ordena na aplicação antes de paginar. Para o volume de uma vendinha isso é suficiente; com muitos milhares de clientes, o ideal seria guardar o valor em centavos (inteiro) e ordenar no banco.
- **Busca por nome**: usa `LIKE` do SQLite, que ignora maiúsculas e minúsculas apenas para letras sem acento.
- **Regras críticas também no banco**: CPF único e uma dívida em aberto por cliente são garantidos por índices únicos, protegendo contra requisições simultâneas.
- **Arquivos `-wal` e `-shm`**: o EF Core liga o modo WAL do SQLite, que cria esses dois arquivos ao lado do `vendinha.db`. Eles fazem parte do mesmo banco.
