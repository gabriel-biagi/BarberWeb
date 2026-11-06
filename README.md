# BarberWeb

API REST para gerenciamento de uma barbearia, desenvolvida com **ASP.NET Core 8**, **Entity Framework Core 8** e **MySQL**. Projeto de estudo voltado à modelagem de domínio, regras de negócio, persistência, tratamento de erros e cálculo de disponibilidade de horários.

> **Status:** projeto em desenvolvimento. Atualmente, somente o recurso de **Profissional** possui endpoints HTTP. As demais entidades e alguns serviços já existem no código, mas ainda não formam fluxos completos disponíveis pela API.

---

## Navegação

- [Visão geral](#visão-geral)
  - [Implementado no código](#implementado-no-código)
  - [Ainda não implementado como fluxo completo](#ainda-não-implementado-como-fluxo-completo)
- [Arquitetura](#arquitetura)
  - [Direção das dependências](#direção-das-dependências)
  - [Fluxo de uma requisição](#fluxo-de-uma-requisição)
- [Domínio](#domínio)
  - [Entidades](#entidades)
  - [Regras de domínio presentes no código](#regras-de-domínio-presentes-no-código)
- [Disponibilidade e datas](#disponibilidade-e-datas)
  - [`DateTimeProvider`](#datetimeprovider)
  - [Como o cálculo atual de disponibilidade funciona](#como-o-cálculo-atual-de-disponibilidade-funciona)
  - [Fluxo esperado para criar um agendamento](#fluxo-esperado-para-criar-um-agendamento)
- [Persistência](#persistência)
  - [Índices de unicidade](#índices-de-unicidade)
- [API](#api)
  - [Padrão de retorno e tratamento de erros](#padrão-de-retorno-e-tratamento-de-erros)
- [Stack](#stack)
- [Status atual e pendências técnicas](#status-atual-e-pendências-técnicas)
  - [Já presente](#já-presente)
  - [Próximas implementações](#próximas-implementações)
  - [Inconsistências e pontos de manutenção identificados](#inconsistências-e-pontos-de-manutenção-identificados)
- [Configuração e execução](#configuração-e-execução)
  - [Pré-requisitos](#pré-requisitos)
  - [Clone o projeto](#1-clone-o-projeto)
  - [Configure a connection string com User Secrets](#2-configure-a-connection-string-com-user-secrets)
  - [Aplique as migrations](#3-aplique-as-migrations)
  - [Execute a API](#4-execute-a-api)
  - [Execute os testes](#5-execute-os-testes)
- [Objetivo do projeto](#objetivo-do-projeto)

---

## Visão geral

O sistema está sendo construído para permitir que uma barbearia gerencie profissionais, serviços, horários de funcionamento, clientes e agendamentos.

### Implementado no código

- Endpoints de profissionais para listar, consultar por ID, cadastrar e remover.
- Entidades de domínio para clientes, profissionais, serviços, ofertas de serviços, horários de funcionamento e agendamentos.
- Associação entre profissional e serviço por meio de `ProfessionalServiceOffering`, com preço e duração próprios para cada combinação.
- Modelagem de horários semanais por profissional, permitindo mais de um bloco de funcionamento no mesmo dia.
- Serviços e repositórios para consultar horários de funcionamento, duração de uma oferta e agendamentos existentes.
- `AvailableHoursService`, que calcula possíveis horários a partir do expediente, da duração do serviço e dos agendamentos consultados. Esse cálculo ainda não possui endpoint e tem limitações descritas em [Disponibilidade e datas](#disponibilidade-e-datas).
- Persistência com EF Core e MySQL, incluindo migrations e índices de unicidade.
- Retorno de operações da aplicação com `Result` e `Result<T>`, além de tratamento global de exceções na API.
- Testes de domínio para `Professional`. Os testes de `ProfessionalService` precisam ser atualizados para refletir o uso atual de `Result<T>`; veja [execução dos testes](#5-execute-os-testes).

### Ainda não implementado como fluxo completo

- Endpoints e casos de uso para clientes, serviços, ofertas e horários de funcionamento.
- Endpoint para consultar horários disponíveis.
- Criação de agendamentos com validação completa de expediente, horários passados e conflitos.
- Remarcação e cancelamento de agendamentos.
- Histórico de alterações de agendamento.
- Autenticação e autorização.

---

## Arquitetura

O projeto utiliza uma arquitetura em camadas, com separação entre domínio, aplicação, infraestrutura e API.

```text
BarberWeb/
├── src/
│   ├── BarberWeb.Api/             # Controllers, middleware e configuração HTTP
│   ├── BarberWeb.Application/    # Services, DTOs, Result Pattern e casos de uso
│   ├── BarberWeb.Domain/         # Entidades, regras de domínio, exceções e contratos
│   └── BarberWeb.Infrastructure/ # EF Core, DbContext, migrations e repositórios
└── tests/
    ├── BarberWeb.Domain.Tests/       # Testes das entidades de domínio
    └── BarberWeb.Application.Tests/  # Testes dos services
```

### Direção das dependências

```text
Api → Application → Domain
Api → Infrastructure → Domain
```

A API referencia `Application` e `Infrastructure` para configurar a injeção de dependência. `Application` e `Infrastructure` dependem de `Domain`. O projeto `Domain` não depende das demais camadas.

### Fluxo de uma requisição

```text
HTTP Request
    ↓
Controller (Api)
    ↓
Service / caso de uso (Application)
    ↓
Interface de repositório (Domain)
    ↓
Implementação EF Core (Infrastructure)
    ↓
MySQL
```

---

## Domínio

### Entidades

| Entidade | Responsabilidade |
|---|---|
| `Customer` | Dados do cliente: nome, email e telefone. Mantém a coleção de agendamentos. |
| `Professional` | Dados do profissional: nome, email e telefone. O telefone é normalizado para conter somente dígitos no construtor. |
| `Service` | Catálogo de serviços, com nome e descrição. Não guarda preço nem duração fixos. |
| `ProfessionalServiceOffering` | Associação entre profissional e serviço. Guarda o preço e a duração praticados por aquele profissional. |
| `OpeningHours` | Bloco semanal de funcionamento de um profissional, identificado por dia da semana, horário inicial e horário final. Um dia pode ter vários blocos, por exemplo, antes e depois do almoço. |
| `SchedulingHours` | Agendamento de um cliente para uma oferta específica, com início e fim representados por `DateTimeOffset`. Também guarda `ProfessionalId` para permitir o índice composto configurado no EF Core. |

`SchedulingHours` referencia `ProfessionalServiceOffering`, e não diretamente `Service`, porque a oferta define o preço e a duração aplicáveis àquele profissional. O modelo atual ainda não salva um snapshot do preço ou da duração no momento do agendamento. Portanto, alterar uma oferta no futuro pode afetar os dados consultados por meio dessa relação.

### Regras de domínio presentes no código

- `Professional` e `Customer`: nome entre 3 e 60 caracteres e email entre 5 e 60 caracteres.
- `Professional`: telefone normalizado para dígitos e validado com 10 a 15 dígitos.
- `Customer`: telefone validado com 10 a 15 caracteres, sem a mesma normalização aplicada a `Professional`.
- `Service`: nome entre 3 e 60 caracteres e descrição entre 5 e 200 caracteres.
- `ProfessionalServiceOffering`: preço entre `0.01` e `9999.99` e duração maior que zero.
- `OpeningHours`: horário final deve ser posterior ao horário inicial.
- `SchedulingHours`: instante final deve ser posterior ao instante inicial.
- Relacionamentos obrigatórios são verificados contra referências nulas nos construtores correspondentes.
- Email e telefone de `Professional` possuem índices únicos no banco.

As validações de formato também podem ocorrer na API. Por exemplo, `ProfessionalRequest` utiliza `[EmailAddress]` para o email. As regras da entidade e as validações do DTO não são exatamente a mesma camada de validação.

---

## Disponibilidade e datas

A modelagem separa três conceitos de tempo:

| Tipo | Uso no projeto | Exemplo |
|---|---|---|
| `DateOnly` | Data consultada para encontrar horários | `2026-11-10` |
| `TimeOnly` | Horários recorrentes de funcionamento e horários apresentados ao cliente | `08:00` |
| `DateTimeOffset` | Instantes reais de início e fim de um agendamento | `2026-11-10T09:30:00-04:00` |

`OpeningHours` usa `DayOfWeek`, `TimeOnly` e `TimeOnly`: representa uma regra semanal, não uma data específica. `SchedulingHours` usa `DateTimeOffset` porque representa um compromisso que ocorre em um dia e instante determinados.

### `DateTimeProvider`

A abstração `IDateTimeProvider`, implementada por `DateTimeProvider`, centraliza operações de data e fuso horário. O fuso configurado atualmente é `America/Campo_Grande`, com fallback para o identificador Windows `Central Brazilian Standard Time`.

Os métodos disponíveis são:

- `GetCurrentInstant()`: obtém o instante atual em UTC por meio de `TimeProvider`.
- `GetUtcRangeForLocalDay(DateOnly)`: converte o início do dia local e o início do dia seguinte para formar o intervalo UTC daquele dia.
- `ConvertLocalToUtc(DateOnly, TimeOnly)`: combina uma data e um horário local e converte o resultado para UTC.
- `ConvertUtcToLocalTime(DateTimeOffset)`: converte um instante UTC para o fuso da barbearia e retorna somente a parte de horário (`TimeOnly`).

O banco MySQL recebe `DateTimeOffset` como `datetime(6)` nas migrations atuais. Esse tipo SQL não preserva o offset/fuso horário. Por isso, a convenção da aplicação deve ser armazenar e comparar os instantes em UTC e converter para o horário local da barbearia na entrada e na apresentação. Essa convenção ainda precisa ser aplicada de forma consistente em todos os fluxos de gravação, que não foram implementados.

### Como o cálculo atual de disponibilidade funciona

O método `AvailableHoursService.GetAvailableSlotsByProfessionalAndDateAsync` recebe o ID do profissional, uma data local (`DateOnly`) e o ID do serviço. Em linhas gerais, ele:

1. Descobre o dia da semana a partir da data consultada.
2. Obtém o intervalo UTC correspondente àquele dia local.
3. Busca os blocos de funcionamento do profissional para aquele dia da semana.
4. Busca os agendamentos encontrados no intervalo consultado.
5. Consulta a duração da oferta daquele serviço para aquele profissional.
6. Percorre cada bloco de funcionamento e gera intervalos cuja duração é igual à duração do serviço.
7. Desconsidera os intervalos que se sobrepõem aos agendamentos retornados pela consulta.

A regra de sobreposição usada no cálculo é equivalente a:

```csharp
candidateStart < appointmentEnd && candidateEnd > appointmentStart
```

Isso impede, por exemplo, oferecer `09:00–09:30` quando já existe um atendimento `09:15–09:45`.

**Importante: o cálculo é uma primeira implementação e ainda não deve ser tratado como uma validação definitiva para confirmar agendamentos.** Há limitações no código atual:

- `EfSchedulingHoursRepository` filtra os registros pelo `StartDate` estar dentro do dia (`StartDate >= utcStart && StartDate < utcEnd`). Isso não encontra um agendamento iniciado antes do dia consultado e que ainda avance sobre ele. Uma consulta por sobreposição precisa considerar também `EndDate > utcStart`.
- `AvailableHoursService` converte os instantes dos agendamentos para `TimeOnly`, descartando a data antes de comparar os intervalos. Para comparações robustas, os intervalos candidatos e existentes devem ser comparados como instantes completos, considerando o fuso da barbearia.
- `GetCurrentInstant()` existe, mas o cálculo atual não o utiliza para remover horários que já passaram no dia consultado.
- O passo de geração dos horários é a própria duração do serviço. Assim, um serviço de 45 minutos gera opções em intervalos de 45 minutos. Caso a regra desejada seja oferecer horários a cada 15 minutos, por exemplo, o passo precisa ser uma configuração separada da duração do atendimento.
- A consulta de disponibilidade não reserva o horário. A confirmação futura precisará validar novamente o conflito no servidor e tratar requisições concorrentes.
- Os blocos de funcionamento não podem atravessar a meia-noite na validação atual, pois `EndTime` precisa ser maior que `StartTime`.

### Fluxo esperado para criar um agendamento

Quando o caso de uso de criação for implementado, o fluxo deverá validar no servidor:

1. Se o cliente, o profissional e a oferta existem.
2. Se a oferta pertence ao profissional informado.
3. Se o profissional trabalha no dia escolhido e se o atendimento cabe integralmente em um bloco de funcionamento.
4. Se a data e o horário escolhidos ainda estão no futuro, usando o instante atual.
5. Se o intervalo completo do atendimento não se sobrepõe a outro agendamento do profissional.
6. Se a gravação pode ser concluída sem conflito com outra requisição concorrente.

O cliente deverá informar a data e o horário no contexto local da barbearia. O servidor deverá obter a duração a partir de `ProfessionalServiceOffering`, calcular o fim, converter os instantes para UTC e persistir o agendamento. A duração não deve ser aceita como valor confiável enviado pelo cliente.

---

## Persistência

- **ORM:** Entity Framework Core 8.
- **Banco de dados:** MySQL, usando `Pomelo.EntityFrameworkCore.MySql`.
- **Migrations:** `InitialMigration` e `AddUniqueIndexesOnProfessional`.
- **Tabelas criadas pela migration inicial:** `Customers`, `Professionals`, `Services`, `OpeningHours`, `ProfessionalOfferings` e `Appointments`.

### Índices de unicidade

- `Professionals`: índices únicos para `Email` e `PhoneNumber`.
- `Appointments`: índice único composto por `(ProfessionalId, StartDate, EndDate)`.

O índice de `Appointments` impede que dois registros tenham a mesma combinação de profissional, início e fim. **Ele não impede toda e qualquer sobreposição de intervalos.** A regra de sobreposição exige uma validação própria, além de tratamento de concorrência na gravação.

---

## API

A rota do controller atual é `/api/professional` (o roteamento do ASP.NET Core não diferencia maiúsculas de minúsculas).

| Método | Rota | Descrição | Respostas documentadas |
|---|---|---|---|
| `GET` | `/api/professional` | Lista os profissionais | `200` |
| `GET` | `/api/professional/{id}` | Busca um profissional por ID | `200`, `400`, `404` |
| `POST` | `/api/professional` | Cadastra um profissional | `201`, `400`, `409` |
| `DELETE` | `/api/professional/{id}` | Remove um profissional | `204`, `400`, `404` |

O corpo do `POST` recebe `name`, `email` e `phoneNumber`. A resposta de profissional contém `id`, `name` e `email`.

Exemplo de corpo:

```json
{
  "name": "Ana Souza",
  "email": "ana@email.com",
  "phoneNumber": "11955554444"
}
```

A documentação das respostas acima reflete os atributos `ProducesResponseType` do controller. Erros inesperados ainda podem resultar em `500` pelo middleware global.

### Padrão de retorno e tratamento de erros

Os services usam `Result` e `Result<T>` para representar sucesso ou falha sem exigir exceções para resultados esperados. O `Result<T>` disponibiliza `Value` em caso de sucesso e `Error` em caso de falha.

O `ErrorType` (singular) possui os seguintes mapeamentos na API:

| `ErrorType` | HTTP | Significado |
|---|---:|---|
| `InvalidEntry` | `400` | Entrada inválida |
| `NotFound` | `404` | Recurso não encontrado |
| `Conflict` | `409` | Conflito, como email ou telefone já cadastrado |
| `BusinessRule` | `422` | Violação de regra de negócio |

O middleware global também trata exceções não convertidas em `Result`:

| Exceção | HTTP |
|---|---:|
| `InvalidInputException` | `400` |
| `NotFoundException` | `404` |
| `BusinessException` | `422` |
| Exceção inesperada | `500` |

As respostas seguem `ProblemDetails` ou `ValidationProblemDetails`, com propriedades como `status`, `title`, `detail`, `instance` e `traceId`. O middleware atual não inclui `stackTrace` na resposta.

No cadastro de profissional, uma violação de índice único do MySQL (código `1062`) é convertida pelo repositório em `UniqueConstraintViolationException`. O service captura essa exceção e retorna `ErrorType.Conflict`, que o controller transforma em HTTP `409`.

---

## Stack

| Categoria | Tecnologias |
|---|---|
| Linguagem e runtime | C# / .NET 8 |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Banco de dados | MySQL |
| Provider EF Core | Pomelo.EntityFrameworkCore.MySql 8 |
| Documentação | Swagger / OpenAPI — Swashbuckle |
| Testes | xUnit + Moq |
| Controle de versão | Git / GitHub |

---

## Status atual e pendências técnicas

### Já presente

- [x] Entidades de domínio, exceções e interfaces de repositório.
- [x] `AppDbContext`, migrations e índices configurados.
- [x] Registro de repositórios, services e `DateTimeProvider` no container de DI.
- [x] Implementações EF Core dos repositórios.
- [x] Cálculo inicial de disponibilidade em `AvailableHoursService`.
- [x] Controller, DTOs e tratamento de erros de `Professional`.
- [x] Testes unitários da entidade `Professional`.

### Próximas implementações

- [ ] Atualizar os testes de `ProfessionalService` para o contrato atual de `Result<T>`.
- [ ] Criar controllers e DTOs para clientes, serviços, ofertas e horários de funcionamento.
- [ ] Expor o endpoint de consulta de horários disponíveis.
- [ ] Corrigir a busca de agendamentos por sobreposição e comparar intervalos completos, sem descartar a data.
- [ ] Filtrar horários que já passaram.
- [ ] Implementar criação de agendamentos com validação e proteção contra concorrência.
- [ ] Implementar remarcação e cancelamento.
- [ ] Definir como preservar o preço e a duração contratados quando uma oferta mudar.
- [ ] Implementar histórico de movimentação.
- [ ] Implementar autenticação e autorização.

### Inconsistências e pontos de manutenção identificados

- Existe um enum `ErrorTypes` (plural), mas o padrão `Result` e os services utilizam `ErrorType` (singular). O enum plural aparenta ser legado e não participa do fluxo atual.
- A validação do telefone de `Professional` normaliza os dígitos no domínio, enquanto `ProfessionalRequest` aplica `[StringLength]` ao texto recebido antes dessa normalização. Números com código do país e formatação extensa podem ser aceitos pela entidade e rejeitados pelo DTO.
- `Customer` não aplica a mesma normalização de telefone usada por `Professional`.
- `SchedulingHoursService` recebe `IOpeningHoursService` no construtor, mas o campo injetado não é utilizado no código atual.
- Os testes em `ProfessionalServiceTests` ainda refletem uma implementação anterior: esperam exceções em cenários que agora retornam `Result<T>` e acessam propriedades diretamente no resultado, em vez de usar `Value`. Eles precisam ser reescritos antes de considerar a suíte alinhada ao código atual.

---

## Configuração e execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- MySQL em execução e acessível.
- Ferramenta `dotnet-ef`:

```bash
dotnet tool install --global dotnet-ef
```

### 1. Clone o projeto

```bash
git clone https://github.com/gabriel-biagi/BarberWeb.git
cd BarberWeb
```

### 2. Configure a connection string com User Secrets

O projeto `BarberWeb.Api` já possui `UserSecretsId`. Configure a conexão sem colocar a senha no `appsettings.json`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=BarberWebDB;Uid=root;Pwd=suasenha" --project src/BarberWeb.Api
```

O MySQL precisa estar acessível durante a inicialização, pois `Program.cs` utiliza `ServerVersion.AutoDetect` para configurar o provider.

### 3. Aplique as migrations

```bash
dotnet ef database update --project src/BarberWeb.Infrastructure --startup-project src/BarberWeb.Api
```

### 4. Execute a API

```bash
dotnet run --project src/BarberWeb.Api --launch-profile https
```

No ambiente Development, o Swagger fica disponível em `https://localhost:7250/swagger`. O perfil HTTP utiliza `http://localhost:5081/swagger`.

### 5. Execute os testes

Na raiz da solução:

```bash
dotnet test
```

> **Atenção:** os testes de `ProfessionalService` estão desatualizados em relação ao `Result<T>` atual e precisam ser corrigidos. Não considere a suíte validada até atualizar esses testes e executar `dotnet test` com sucesso.

---

## Objetivo do projeto

Projeto de estudo pessoal desenvolvido para praticar modelagem de domínio e construção de uma API em camadas. O foco inclui injeção de dependência, Entity Framework Core, relacionamentos, índices e constraints, regras de negócio, tratamento de erros com `Result`, testes automatizados e, principalmente, lógica de disponibilidade e consistência de agendamentos.
