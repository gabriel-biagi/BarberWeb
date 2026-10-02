# BarberWeb

API REST para agendamento em barbearias, em desenvolvimento com **ASP.NET Core Web API** e **Entity Framework Core**. Projeto de estudo construído do zero, aplicando Clean Architecture, Repository Pattern e boas práticas de modelagem de domínio.

> ⚠️ Projeto em desenvolvimento ativo. Nem todas as camadas estão conectadas ainda — ver "Status atual" abaixo.

---

## Visão geral

- Cadastro de clientes, profissionais e serviços
- Serviços oferecidos por profissional, com preço e duração próprios por par (Profissional + Serviço)
- Horário de funcionamento por profissional, com blocos por dia da semana
- Cálculo de disponibilidade de horários sob demanda (não armazenado)
- Agendamento, remarcação e cancelamento com histórico de movimentação
- Validação de conflito de horário no servidor

---

## Arquitetura

Arquitetura em camadas (Clean Architecture), separando domínio, aplicação, infraestrutura e API.

```text
BarberWeb/
├── BarberWeb.Api             → Controllers, middleware e configuração HTTP
├── BarberWeb.Application     → Services, DTOs e casos de uso
├── BarberWeb.Domain          → Entidades, interfaces de repositório e regras de negócio
└── BarberWeb.Infrastructure  → EF Core, DbContext e implementações de repositório
```

### Direção das dependências

```
Api → Application → Domain
Infrastructure → Domain
Api → Infrastructure (exceção: necessário para registrar o DbContext e os serviços no DI, em Program.cs)
```

Nenhuma camada depende de `Infrastructure` além da `Api`, e `Domain` não depende de nenhuma outra camada.

---

## Domínio

### Entidades

- **Customer** — Nome, email, telefone. Possui uma lista de agendamentos.
- **Professional** — Nome, email, telefone. Oferece serviços através de `ProfessionalServiceOffering`.
- **Service** — Nome e descrição de um serviço (ex.: "Corte", "Barba"). Sem preço ou duração fixos.
- **ProfessionalServiceOffering** — Associação entre `Professional` e `Service`, carregando o **preço** e a **duração** praticados por aquele profissional especificamente. Permite que o mesmo serviço tenha preço/duração diferentes entre profissionais.
- **OpeningHours** — Bloco de funcionamento de um profissional em um dia da semana (ex.: Segunda, 08:00–11:00). Um profissional pode ter múltiplos blocos por dia e nenhum em dias sem atendimento.
- **SchedulingHours** — Agendamento de um cliente com uma oferta específica (`ProfessionalServiceOffering`), com início e fim (`DateTimeOffset`). Referencia a oferta — não o serviço ou o profissional diretamente — para preservar o preço e a duração vigentes no momento do agendamento.

### Regras de negócio implementadas

- Nome de cliente/profissional entre 3 e 60 caracteres; email entre 5 e 60; telefone entre 10 e 15.
- Nome de serviço entre 3 e 60 caracteres; descrição entre 5 e 200.
- Preço de uma oferta entre 0.01 e 9999.99.
- Horário de funcionamento: fim não pode ser menor ou igual ao início.
- Agendamento: fim não pode ser menor ou igual ao início.
- Nenhuma entidade aceita referência nula onde uma relação é obrigatória (validado no construtor).

### Regras de negócio decididas, ainda não implementadas em código

- Cálculo de horários disponíveis por profissional + serviço escolhido, respeitando a duração específica da oferta.
- Checagem de sobreposição de horários (`A.Início < B.Fim E B.Início < A.Fim`) antes de aceitar um novo agendamento.
- Histórico de movimentação registrando toda escrita em agendamento (criar, remarcar, cancelar), em transação com a escrita principal.
- Remarcação como atualização do registro existente; cancelamento como exclusão real (sem soft-delete).

---

## Persistência

- **ORM**: Entity Framework Core 8
- **Banco de dados**: MySQL (via Pomelo.EntityFrameworkCore.MySql)
- **Constraint de unicidade**: índice único composto em `SchedulingHours` por `(ProfessionalId, StartDate, EndDate)`, como segunda camada de defesa contra conflito de horário exato (a checagem de sobreposição real fica na camada de aplicação, pois `HasIndex` não expressa comparações de intervalo).

---

## Stack

| Categoria | Tecnologias |
|---|---|
| Linguagem | C# / .NET 8 |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Banco | MySQL + Pomelo.EntityFrameworkCore.MySql |
| Documentação | Swagger / OpenAPI — Swashbuckle |
| Controle de versão | Git / GitHub |

---

## Status atual

Em desenvolvimento. O que já existe:

- ✅ Entidades de domínio e interfaces de repositório (`Domain`)
- ✅ `AppDbContext` com os `DbSet` e a constraint de unicidade (`Infrastructure`)
- ⬜ Configuração de conexão com o banco e registro de serviços no `Program.cs`
- ⬜ Implementações concretas dos repositórios (`Infrastructure`)
- ⬜ `SchedulingHoursService` com o cálculo de disponibilidade e checagem de conflito (`Application`)
- ⬜ Controllers e DTOs (`Api`)
- ⬜ Entidade de Histórico de Movimentação
- ⬜ Autenticação (planejada para uma fase posterior, fora do escopo do MVP)

---

## Configuração e execução

### 1. Clone o projeto

```bash
git clone https://github.com/gabriel-biagi/BarberWeb.git
cd BarberWeb
```

### 2. Configure User Secrets

Dentro de `BarberWeb.Api`:

```bash
cd BarberWeb.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=BarberWebDB;Uid=root;Pwd=suasenha"
```

### 3. Aplique as migrations

*(pendente — ainda não geradas; requer `AddDbContext` configurado em `Program.cs` primeiro)*

### 4. Execute a API

```bash
dotnet run --project BarberWeb.Api
```

Em desenvolvimento, o Swagger fica disponível em `https://localhost:<porta>/swagger`.

---

## Objetivo do projeto

Projeto de estudo pessoal, construído do zero (sem copiar de projetos anteriores) para praticar: injeção de dependência e seus ciclos de vida, Entity Framework Core (relacionamentos, constraints, transações), modelagem de regras de negócio, e leitura/depuração de código.
