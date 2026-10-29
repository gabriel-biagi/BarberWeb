# BarberWeb

API REST para agendamento em barbearias, desenvolvida com **ASP.NET Core 8** e **Entity Framework Core** (MySQL). Projeto de estudo construído do zero, aplicando Clean Architecture, Repository Pattern, Result Pattern e boas práticas de modelagem de domínio.

> ⚠️ Projeto em desenvolvimento ativo. Hoje apenas o recurso **Profissional** está exposto via API; as demais entidades já existem no domínio e no banco, mas ainda não têm endpoints. Veja "Status atual" abaixo.

---

## Visão geral

**Já implementado**

- CRUD parcial de profissionais via API (listar, buscar por id, criar e remover)
- Modelo de domínio com 6 entidades e validações no construtor
- Serviços oferecidos por profissional, com preço e duração próprios por par (Profissional + Serviço)
- Horário de funcionamento por profissional, com blocos por dia da semana
- Cálculo de horários disponíveis sob demanda (não armazenado), implementado na camada de aplicação — ainda sem endpoint
- Persistência com EF Core + MySQL, com migrations versionadas
- Tratamento de erros padronizado (Result Pattern + middleware global de exceções)
- Testes unitários com xUnit e Moq

**Planejado**

- Endpoints para clientes, serviços, ofertas, horários de funcionamento e agendamentos
- Criação de agendamento com validação de conflito de horário no servidor
- Remarcação e cancelamento com histórico de movimentação

---

## Arquitetura

Arquitetura em camadas (Clean Architecture), separando domínio, aplicação, infraestrutura e API.

```text
BarberWeb/
├── src/
│   ├── BarberWeb.Api             → Controllers, middleware e configuração HTTP
│   ├── BarberWeb.Application     → Services, DTOs, Result Pattern e casos de uso
│   ├── BarberWeb.Domain          → Entidades, exceções, interfaces de repositório e regras de negócio
│   └── BarberWeb.Infrastructure  → EF Core, DbContext, migrations e implementações de repositório
└── tests/
    ├── BarberWeb.Domain.Tests        → Testes das entidades de domínio
    └── BarberWeb.Application.Tests   → Testes dos services (repositórios mockados com Moq)
```

### Direção das dependências

```text
Api → Application → Domain
Infrastructure → Domain
Api → Infrastructure (exceção: necessário para registrar o DbContext e os repositórios no DI, em Program.cs)
```

Nenhuma camada depende de `Infrastructure` além da `Api`, e `Domain` não depende de nenhuma outra camada.

---

## Domínio

### Entidades

- **Customer** — Nome, email, telefone. Possui uma lista de agendamentos.
- **Professional** — Nome, email, telefone. Oferece serviços através de `ProfessionalServiceOffering`. O telefone é normalizado no construtor (somente dígitos são mantidos) e email e telefone são únicos.
- **Service** — Nome e descrição de um serviço (ex.: "Corte", "Barba"). Sem preço ou duração fixos.
- **ProfessionalServiceOffering** — Associação entre `Professional` e `Service`, carregando o **preço** e a **duração** praticados por aquele profissional especificamente. Permite que o mesmo serviço tenha preço/duração diferentes entre profissionais.
- **OpeningHours** — Bloco de funcionamento de um profissional em um dia da semana (ex.: Segunda, 08:00–11:00). Um profissional pode ter múltiplos blocos por dia e nenhum em dias sem atendimento.
- **SchedulingHours** — Agendamento de um cliente com uma oferta específica (`ProfessionalServiceOffering`), com início e fim (`DateTimeOffset`). Referencia a oferta — não o serviço ou o profissional diretamente — como fonte de preço e duração. Guarda também o `ProfessionalId` (redundante com a oferta) para permitir o índice único no banco. Ainda não há *snapshot* de preço/duração caso uma oferta seja alterada no futuro.

### Regras de negócio implementadas

- Nome de cliente/profissional entre 3 e 60 caracteres; email entre 5 e 60; telefone entre 10 e 15 (em `Professional`, contados após a normalização para dígitos).
- Nome de serviço entre 3 e 60 caracteres; descrição entre 5 e 200.
- Preço de uma oferta entre 0.01 e 9999.99; duração da oferta maior que zero.
- Horário de funcionamento: fim não pode ser menor ou igual ao início.
- Agendamento: fim não pode ser menor ou igual ao início.
- Nenhuma entidade aceita referência nula onde uma relação é obrigatória (validado no construtor).
- Email e telefone de profissional não podem se repetir (índices únicos; violações viram `422` na API — ver "Tratamento de erros").
- Cálculo de horários disponíveis por profissional + serviço + data, respeitando a duração específica da oferta e descontando os agendamentos já existentes (`AvailableHoursService`).

### Regras de negócio decididas, ainda não implementadas em código

- Checagem de sobreposição de horários (`A.Início < B.Fim E B.Início < A.Fim`) antes de aceitar um novo agendamento. Hoje a sobreposição só é considerada ao *listar* horários livres; o `SchedulingHoursService` ainda apenas consulta.
- Histórico de movimentação registrando toda escrita em agendamento (criar, remarcar, cancelar), em transação com a escrita principal.
- Remarcação como atualização do registro existente; cancelamento como exclusão real (sem soft-delete).
- Endpoint para consultar os horários disponíveis.

---

## Persistência

- **ORM**: Entity Framework Core 8
- **Banco de dados**: MySQL (via Pomelo.EntityFrameworkCore.MySql)
- **Migrations**: `InitialMigration` (tabelas `Customers`, `Professionals`, `Services`, `OpeningHours`, `ProfessionalOfferings` e `Appointments`) e `AddUniqueIndexesOnProfessional`.
- **Constraints de unicidade**:
  - `Appointments`: índice único composto por `(ProfessionalId, StartDate, EndDate)`, como segunda camada de defesa contra conflito de horário exato (a checagem de sobreposição real ficará na camada de aplicação, pois `HasIndex` não expressa comparações de intervalo).
  - `Professionals`: índices únicos em `Email` e `PhoneNumber`.

---

## API

Base: `/api/professional`

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `GET` | `/api/professional` | Lista os profissionais | `200` |
| `GET` | `/api/professional/{id}` | Busca um profissional por id | `200`, `400`, `404` |
| `POST` | `/api/professional` | Cadastra um profissional | `201`, `400`, `422` |
| `DELETE` | `/api/professional/{id}` | Remove um profissional | `204`, `400`, `404` |

Corpo do `POST`: `name`, `email` e `phoneNumber`. A resposta retorna `id`, `name` e `email`.

### Tratamento de erros

- Os services retornam `Result<T>` (com `ErrorTypes`: `NotFound`, `InvalidEntry`, `Conflict`) e o controller traduz o resultado para o status HTTP.
- Exceções de domínio (`InvalidInputException` → `400`, `BusinessException` → `422`, `NotFoundException` → `404`) e qualquer erro inesperado (`500`) são tratados por um middleware global, que devolve `statusCode`, `message` e `traceId` (e o `stackTrace` apenas em Development).
- Violação de unicidade no MySQL (erro 1062) é convertida em `UniqueConstraintViolationException` no repositório e vira `Conflict` no service (resposta `422`).

---

## Stack

| Categoria | Tecnologias |
|---|---|
| Linguagem | C# / .NET 8 |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Banco | MySQL + Pomelo.EntityFrameworkCore.MySql |
| Documentação | Swagger / OpenAPI — Swashbuckle |
| Testes | xUnit + Moq |
| Controle de versão | Git / GitHub |

---

## Status atual

Em desenvolvimento. O que já existe:

- ✅ Entidades de domínio, exceções e interfaces de repositório (`Domain`)
- ✅ `AppDbContext`, constraints de unicidade e migrations (`Infrastructure`)
- ✅ Conexão com o banco e registro de repositórios e services no `Program.cs`
- ✅ Implementações concretas dos repositórios (`Infrastructure`)
- ✅ `AvailableHoursService` com o cálculo de disponibilidade (`Application`)
- ✅ Controller, DTOs e tratamento de erros de `Professional` (`Api`)
- ✅ Testes unitários da entidade `Professional`
- ⚠️ Testes de `ProfessionalService` ainda esperam exceções; precisam ser atualizados para o padrão `Result<T>`
- ⬜ Controllers e DTOs para `Customer`, `Service`, ofertas, horários de funcionamento e agendamentos
- ⬜ Endpoint de horários disponíveis
- ⬜ `SchedulingHoursService` com criação de agendamento e checagem de conflito (`Application`)
- ⬜ Entidade de Histórico de Movimentação
- ⬜ Autenticação (planejada para uma fase posterior, fora do escopo do MVP)

---

## Configuração e execução

**Pré-requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), MySQL em execução e a ferramenta `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

### 1. Clone o projeto

```bash
git clone https://github.com/gabriel-biagi/BarberWeb.git
cd BarberWeb
```

### 2. Configure User Secrets

O projeto `BarberWeb.Api` já possui um `UserSecretsId`, então basta definir a connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=BarberWebDB;Uid=root;Pwd=suasenha" --project src/BarberWeb.Api
```

> O MySQL precisa estar acessível ao iniciar a API (a versão do servidor é detectada automaticamente na inicialização).

### 3. Aplique as migrations

```bash
dotnet ef database update --project src/BarberWeb.Infrastructure --startup-project src/BarberWeb.Api
```

### 4. Execute a API

```bash
dotnet run --project src/BarberWeb.Api --launch-profile https
```

Em desenvolvimento, o Swagger fica disponível em `https://localhost:7250/swagger` (ou `http://localhost:5081/swagger` com o perfil `http`).

### 5. Execute os testes

```bash
dotnet test
```

---

## Objetivo do projeto

Projeto de estudo pessoal, construído do zero (sem copiar de projetos anteriores) para praticar: injeção de dependência e seus ciclos de vida, Entity Framework Core (relacionamentos, constraints, transações), modelagem de regras de negócio, testes unitários e leitura/depuração de código.
