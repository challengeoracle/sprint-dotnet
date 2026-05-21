# 🏥 Medix — Plataforma de Gestão de Saúde

> Painel administrativo B2B para o gerenciamento do ecossistema de saúde, separando a gestão interna da Medix da gestão das unidades parceiras.

Projeto web desenvolvido com **ASP.NET Core MVC** e **ASP.NET Core Web API**, implementando uma arquitetura multitenant baseada em papéis (Roles), persistência relacional com **Oracle** via Entity Framework Core, persistência NoSQL com **MongoDB** para auditoria, e observabilidade completa com Serilog e OpenTelemetry.

Desenvolvido para o **Challenge FIAP em parceria com a Oracle**.

---

## 👥 Integrantes do Grupo

| Nome | RM | Disciplinas |
|---|---|---|
| Arthur Thomas Mariano de Souza | RM 561061 | IoT & IA Generativa, .NET, Mobile |
| Davi Cavalcanti Jorge | RM 559873 | Compliance & Q.A, DevOps, Mobile |
| Mateus da Silveira Lima | RM 559728 | Banco de Dados, Java, Mobile |

---

## 🎯 Objetivo e Escopo

O Medix é uma plataforma de **dois níveis de acesso**:

1. **Painel da Equipe Medix (Admin):** Back-office para a equipe interna administrar o ciclo de vida das unidades de saúde parceiras.
2. **Portal da Unidade de Saúde (Cliente):** Área dedicada onde cada hospital ou clínica gerencia seus próprios dados operacionais — pacientes e colaboradores.

Isso garante um ecossistema seguro onde **os dados de cada unidade são isolados** e a Equipe Medix mantém o controle administrativo global.

---

## 🏛️ Arquitetura da Solução

O projeto segue os princípios da **Clean Architecture** com separação em quatro camadas bem definidas. A dependência sempre flui de fora para dentro — Apresentação → Aplicação → Domínio, com Infraestrutura implementando as interfaces do Domínio.

```
┌──────────────────────────────────────────────────────────────┐
│                        APRESENTAÇÃO                          │
│                                                              │
│ ┌──────────────────┐  ┌──────────────────┐  ┌─────────────┐  │
│ │   MVC (Admin)    │  │ Area UnidadeSaude│  │  Web API    │  │
│ │ UnidadesMedicas  │  │ Pacientes        │  │ /api/       │  │
│ │ HomeController   │  │ Colaboradores    │  │ unidades    │  │
│ │                  │  │ Dashboard        │  │ pacientes   │  │
│ └─────────┬────────┘  └────────┬─────────┘  │ colaborad.  │  │
│           │                    │            │ auditoria   │  │
└───────────┼────────────────────┼──────────────────┼──────────┘
            │                    │                  │
┌───────────▼────────────────────▼──────────────────▼──────────┐
│                        APLICAÇÃO                             │
│                                                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌────────────┐  │
│  │  IUnidadeService │  │ IPacienteService │  │ IAuditoria │  │
│  │  UnidadeService  │  │ IColaboradorSvc  │  │ Service    │  │
│  └──────────────────┘  └──────────────────┘  └────────────┘  │
│                                                              │
│  DTOs: UnidadeMedicaDto · PacienteDto · ColaboradorDto       │
│  ViewModels: DashboardViewModel · CreateUnidadeViewModel     │
└───────────────────────────────────┬──────────────────────────┘
                                    │
┌───────────────────────────────────▼──────────────────────────┐
│                        INFRAESTRUTURA                        │
│                                                              │
│  ┌─────────────────────────┐   ┌──────────────────────────┐  │
│  │   EF Core (Oracle)      │   │   MongoDB                │  │
│  │  ApplicationDbContext   │   │  MongoDbContext          │  │
│  │  Migrations             │   │  LogsAuditoria (coleção) │  │
│  └─────────────────────────┘   └──────────────────────────┘  │
│                                                              │
│  Repositórios:                                               │
│  IRepository<T> → Repository<T> (genérico)                   │
│  IUnidadeMedicaRepository → UnidadeMedicaRepository          │
│  IPacienteRepository     → PacienteRepository                │
│  IColaboradorRepository  → ColaboradorRepository             │
│                                                              │
│  ASP.NET Core Identity · Health Checks · Serilog             │
│  OpenTelemetry (Tracing + Metrics + Prometheus)              │
└───────────────────────────────────┬──────────────────────────┘
                                    │
┌───────────────────────────────────▼──────────────────────────┐
│                          DOMÍNIO                             │
│                                                              │
│  Entidades: UnidadeMedica · Paciente · Colaborador           │
│  Enums:     StatusUnidade · TipoColaborador                  │
│  Audit:     LogAuditoria (documento MongoDB)                 │
└──────────────────────────────────────────────────────────────┘
```

### Diagrama de dependências simplificado (Mermaid)

```mermaid
graph TD
    subgraph Apresentação
        MVC[MVC Admin]
        AREA[Area UnidadeSaude]
        API[Web API REST]
    end

    subgraph Aplicação
        SVC[Services + Interfaces]
        AUDIT[AuditoriaService]
        DTO[DTOs / ViewModels]
    end

    subgraph Infraestrutura
        EF[EF Core + Oracle]
        MONGO[MongoDB]
        REPO[Repositories]
        ID[ASP.NET Identity]
    end

    subgraph Domínio
        ENT[Entidades + Enums]
        LOG[LogAuditoria]
    end

    MVC --> SVC
    AREA --> SVC
    API --> SVC
    API --> AUDIT
    SVC --> REPO
    AUDIT --> MONGO
    REPO --> EF
    EF --> ENT
    MONGO --> LOG
```

---

## 📡 Endpoints da API REST

### Base URL: `/api`

Todos os endpoints da API exigem autenticação. O papel `EquipeMedix` tem acesso total; o papel `UnidadeSaude` acessa apenas os endpoints das suas próprias unidades.

---

### 🏥 Unidades Médicas — `/api/unidades`

| Método | Rota | Autorização | Descrição |
|---|---|---|---|
| `GET` | `/api/unidades` | Autenticado | Lista paginada com filtros e HATEOAS |
| `GET` | `/api/unidades/{id}` | Autenticado | Busca unidade por ID |
| `POST` | `/api/unidades` | Autenticado | Cria nova unidade médica |
| `PUT` | `/api/unidades/{id}` | Autenticado | Atualiza unidade médica |
| `DELETE` | `/api/unidades/{id}` | Autenticado | Remove unidade médica |

**Query params do GET `/api/unidades`:**

| Parâmetro | Tipo | Padrão | Descrição |
|---|---|---|---|
| `nome` | string | — | Filtro parcial por nome |
| `status` | enum | — | `Ativa`, `Inativa`, `Suspensa`, `EmTeste` |
| `sortBy` | string | `Nome` | Campo de ordenação |
| `sortDirection` | string | `ASC` | `ASC` ou `DESC` |
| `page` | int | `1` | Página atual |
| `pageSize` | int | `10` | Itens por página |

**Exemplo de resposta com HATEOAS:**
```json
{
  "items": [
    {
      "id": 1,
      "nome": "Hospital Central",
      "cnpj": "00.000.000/0001-00",
      "status": "Ativa",
      "dataCadastro": "2024-01-01T00:00:00Z",
      "links": [
        { "href": "/api/unidades/1", "rel": "self",   "method": "GET" },
        { "href": "/api/unidades/1", "rel": "update", "method": "PUT" },
        { "href": "/api/unidades/1", "rel": "delete", "method": "DELETE" }
      ]
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "links": [
    { "href": "/api/unidades?page=1", "rel": "self", "method": "GET" }
  ]
}
```

---

### 👤 Pacientes — `/api/unidades/{unidadeId}/pacientes`

| Método | Rota | Autorização | Descrição |
|---|---|---|---|
| `GET` | `/api/unidades/{unidadeId}/pacientes` | Autenticado | Lista paginada de pacientes |
| `GET` | `/api/unidades/{unidadeId}/pacientes/{id}` | Autenticado | Busca paciente por ID |
| `POST` | `/api/unidades/{unidadeId}/pacientes` | Autenticado | Registra novo paciente |
| `PUT` | `/api/unidades/{unidadeId}/pacientes/{id}` | Autenticado | Atualiza paciente |
| `DELETE` | `/api/unidades/{unidadeId}/pacientes/{id}` | Autenticado | Remove paciente |

---

### 👨‍⚕️ Colaboradores — `/api/unidades/{unidadeId}/colaboradores`

| Método | Rota | Autorização | Descrição |
|---|---|---|---|
| `GET` | `/api/unidades/{unidadeId}/colaboradores` | Autenticado | Lista paginada de colaboradores |
| `GET` | `/api/unidades/{unidadeId}/colaboradores/{id}` | Autenticado | Busca colaborador por ID |
| `POST` | `/api/unidades/{unidadeId}/colaboradores` | Autenticado | Registra novo colaborador |
| `PUT` | `/api/unidades/{unidadeId}/colaboradores/{id}` | Autenticado | Atualiza colaborador |
| `DELETE` | `/api/unidades/{unidadeId}/colaboradores/{id}` | Autenticado | Remove colaborador |

**Query params do GET colaboradores:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `nome` | string | Filtro parcial por nome |
| `cargo` | enum | `Medico`, `Enfermeiro`, `Tecnico`, `Administrativo` |
| `page` | int | Página atual |
| `pageSize` | int | Itens por página |

---

### 📋 Auditoria — `/api/auditoria` (NoSQL — MongoDB)

> Acesso restrito ao papel `EquipeMedix`.

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/auditoria` | Últimos N logs do sistema (padrão 50, máximo 200) |
| `GET` | `/api/auditoria/{entidade}/{id}` | Histórico de um registro específico |

**Query params:**

| Parâmetro | Tipo | Padrão | Descrição |
|---|---|---|---|
| `limite` | int | `50` | Quantidade de logs (máx. 200) |

**Entidades válidas:** `UnidadeMedica`, `Paciente`, `Colaborador`

**Exemplo de documento de auditoria:**
```json
{
  "id": "6632a1f2e4b0a1c2d3e4f5a6",
  "entidade": "UnidadeMedica",
  "entidadeId": 1,
  "operacao": "UPDATE",
  "realizadoPor": "admin@medix.com",
  "realizadoEm": "2024-05-01T14:30:00Z",
  "detalhe": {
    "nome": "Hospital Central Atualizado",
    "status": "Ativa"
  }
}
```

---

### 🩺 Monitoramento

| Endpoint | Descrição |
|---|---|
| `GET /health` | Status completo (Oracle + MongoDB) |
| `GET /health/ready` | Prontidão (Oracle + MongoDB) |
| `GET /health/live` | Processo em execução |
| `GET /metrics` | Métricas Prometheus |
| `GET /swagger` | Documentação interativa OpenAPI |

---

## 🍃 MongoDB — Configuração

O Medix usa MongoDB para persistência dos logs de auditoria. Configure via User Secrets:

```bash
cd Medix

# MongoDB local
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb://localhost:27017"

# ou

# MongoDB Atlas (nuvem)
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://usuario:senha@cluster.mongodb.net/"
```

| Campo | Valor padrão |
|---|---|
| Database | `MedixAudit` |
| Collection | `LogsAuditoria` |

---

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8** + **ASP.NET Core MVC** + **ASP.NET Core Web API**
- **Entity Framework Core 8** + **Oracle.EntityFrameworkCore**
- **MongoDB.Driver 2.29**
- **ASP.NET Core Identity** com Roles

### Observabilidade
- **Serilog** — logging estruturado (Console + arquivo rotativo diário)
- **OpenTelemetry** — tracing (AspNetCore, HttpClient, EF Core) e métricas (Runtime, Prometheus)
- **ASP.NET Core Health Checks** — Oracle e MongoDB

### Testes
- **xUnit** — framework de testes
- **Moq** — mocking
- **Microsoft.AspNetCore.Mvc.Testing** — testes de integração com `WebApplicationFactory`
- **EF Core InMemory** — banco de dados em memória para testes de repositório

### Frontend
- **Bootstrap 5**, **Chart.js**, **iMask.js**

### Ferramentas
- Visual Studio 2022, Git & GitHub, Postman

---

## 🔐 Configuração de Credenciais

A connection string Oracle e a string do MongoDB **não são versionadas**. Configure via User Secrets:

```bash
cd Medix
dotnet user-secrets init

# Oracle (FIAP)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL;"

# MongoDB
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb://localhost:27017"
```

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- Visual Studio 2022
- Acesso ao Oracle FIAP (ou Oracle local)
- MongoDB (local ou Atlas)

### Passo a passo

```bash
# 1. Clone o repositório
git clone https://github.com/challengeoracle/sprint-dotnet.git
cd sprint-dotnet

# 2. Configure as credenciais (ver seção acima)

# 3. Restaure as dependências
dotnet restore

# 4. Execute a aplicação
dotnet run --project Medix
```

O `Program.cs` aplica as migrations automaticamente na inicialização, cria os papéis `EquipeMedix` e `UnidadeSaude`, e cria o usuário admin padrão.

### 🔑 Primeiro acesso

| Campo | Valor |
|---|---|
| Email | `admin@medix.com` |
| Senha | `Medix123@` |

### Criar acesso para uma Unidade de Saúde

1. Faça login como admin
2. Acesse **Unidades Médicas → Adicionar Nova**
3. Preencha os dados da unidade, e-mail e senha de acesso
4. Saia e faça login com os dados da nova unidade

---

## 🧪 Testes

```bash
# Todos os testes
dotnet test

# Só unitários
dotnet test Medix.Tests.Unit

# Só integração
dotnet test Medix.Tests.Integration

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

### Cobertura atual

| Camada | Testes |
|---|---|
| Domínio (Models / ViewModels) | ✅ UnidadeMedica, Paciente, Colaborador, CreateUnidadeViewModel, EditUnidadeViewModel |
| Aplicação (Services) | ✅ UnidadeService, PacienteService, AuditoriaService |
| Infraestrutura (Repositories) | ✅ UnidadeMedicaRepository, PacienteRepository, ColaboradorRepository |
| Apresentação (Controllers) | ✅ UnidadesMedicasApiController |
| Integração (API + Health Checks) | ✅ Unidades, Pacientes, Colaboradores, Health Checks |

---

## ✨ Funcionalidades por Sprint

### Sprint 1 — Fundação MVC
- CRUD de Unidades Médicas (admin)
- Autenticação com ASP.NET Core Identity
- Dois papéis: `EquipeMedix` e `UnidadeSaude`
- Dashboards separados por papel
- Login inteligente com redirecionamento por papel

### Sprint 2 — API RESTful
- API REST completa para Unidades, Pacientes e Colaboradores
- Paginação, filtros e ordenação server-side
- HATEOAS com links hipermídia nas respostas
- Rate Limiting por IP (100 req/min)
- Documentação Swagger/OpenAPI

### Sprint 3 — Observabilidade e Testes
- Serilog com enriquecimento (MachineName, ThreadId)
- OpenTelemetry (Tracing + Métricas + Prometheus)
- Health Checks em `/health`, `/health/ready`, `/health/live`
- Testes unitários (xUnit + Moq)
- Testes de integração (WebApplicationFactory)

### Sprint 4 — Consolidação
- **MongoDB** integrado para log de auditoria (CREATE, UPDATE, DELETE)
- **Padrão Repository** com `IRepository<T>` genérico e repositórios concretos
- **Tratamento global de exceções** (JSON para `/api/*`, redirect para MVC)
- **Health Check do MongoDB** em `/health/ready`
- `UnidadeMedicaDto` desacoplado do modelo de domínio (sem herança)
- Testes de repositório com EF Core InMemory
- `CustomWebApplicationFactory` com mock do MongoDB para testes de integração
