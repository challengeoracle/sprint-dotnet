# 🏥 Medix - Plataforma de Gestão de Saúde

### Painel administrativo B2B para o gerenciamento do ecossistema de saúde, separando a gestão interna da Medix da gestão das unidades parceiras.

> Projeto web desenvolvido com **ASP.NET Core MVC** e **ASP.NET Core Web API**, implementando uma arquitetura multitenant baseada em papéis (Roles).  
> O sistema provê um painel seguro para a **Equipe Medix** administrar unidades de saúde parceiras e um portal separado (Área) para que cada **Unidade de Saúde** possa gerir os seus próprios pacientes e colaboradores.  
>
> Desenvolvido para o **Challenge FIAP em parceria com a Oracle**.

---

## 👥 Integrantes do Grupo

- **Arthur Thomas Mariano de Souza (RM 561061)** — Responsável pelas matérias de *IoT & IA Generativa, .NET e Mobile*  
- **Davi Cavalcanti Jorge (RM 559873)** — Responsável pelas matérias de *Compliance & Q.A, DevOps e Mobile*  
- **Mateus da Silveira Lima (RM 559728)** — Responsável pelas matérias de *Banco de Dados, Java e Mobile*  

---

## 🎯 Objetivo e Escopo

O objetivo evoluiu para uma plataforma de **dois níveis**:

1. **Painel da Equipe Medix (Admin):** Ferramenta de *back-office* para a equipe interna da Medix administrar o ciclo de vida das unidades de saúde parceiras, incluindo a criação das suas contas de acesso.  
2. **Portal da Unidade de Saúde (Cliente):** Área dedicada e segura onde cada unidade (hospital ou clínica) pode fazer login para gerir os seus próprios dados operacionais, como o registro de pacientes e a gestão de colaboradores.

Isso garante um **ecossistema seguro**, onde os dados de cada unidade são isolados e a **Equipe Medix mantém o controle administrativo global**.

---

### 🔑 (IMPORTANTE) Primeiro Acesso (Equipe Medix)

A aplicação estará disponível em:
`https://localhost:xxxx`

*(OBRIGATÓRIO)* Use o login pré-criado para a equipe para poder começar:

* **Email:** `admin@medix.com`
* **Senha:** `Medix123@`

---

## ✨ Funcionalidades (Sprint 2)

### 🔐 Sistema de Autenticação e Papéis (Roles)

- [x] **Dois Papéis de Acesso:**
  - `EquipeMedix`: Acesso total ao dashboard administrativo principal.
  - `UnidadeSaude`: Acesso restrito ao dashboard e ferramentas da sua própria unidade.
- [x] **Login Inteligente:** Redireciona automaticamente para o dashboard correto (`/Home` ou `/UnidadeSaude/Dashboard`).
- [x] **Registro Privado:** Apenas a Equipe Medix pode criar contas de unidades de saúde.
- [x] **Criação de Conta Admin via Seed:** Usuário `admin@medix.com` é criado automaticamente na inicialização.

---

### 📊 Dashboards (Painéis de Controle)

- [x] **Dashboard da Equipe Medix:** Estatísticas globais e gráfico de distribuição.
- [x] **Dashboard da Unidade de Saúde:** Estatísticas específicas da unidade logada (pacientes, colaboradores, etc.).

---

### 🏥 Gestão (Equipe Medix)

- [x] **CRUD de Unidades Médicas:** Criar, listar, editar e excluir unidades.  
- [x] **Criação de Login da Unidade:** Campos para definir o e-mail e senha do administrador da unidade.  
- [x] **Busca e Paginação:** Filtros, ordenação e paginação *server-side*.  

---

### 🩺 Gestão (Unidade de Saúde)

- [x] **CRUD de Pacientes:** Cada unidade só gerencia seus próprios pacientes.  
- [x] **CRUD de Colaboradores:** Cada unidade só gerencia seus próprios colaboradores.  
- [x] **Isolamento de Dados:** Nenhuma unidade pode acessar dados de outra.  

---

### 🚀 API (RESTful)

- [x] **Endpoint `/api/unidades`:** Retorna dados das unidades de saúde.  
- [x] **Funcionalidades:** Filtros, ordenação e paginação.  
- [x] **HATEOAS:** Links hipermídia nas respostas (self, next, previous, update, delete).  

---

### 🧭 Interface de Usuário

- [x] **Layouts Separados:**  
  `_Layout.cshtml` (Equipe Medix) e `_LayoutUnidade.cshtml` (Unidade de Saúde).  
- [x] **Rotas Personalizadas:** URLs amigáveis (`/unidades/nova`, `/UnidadeSaude/Pacientes/Create`).  
- [x] **Design Profissional:** Interface moderna com **Bootstrap 5** e **Chart.js**.  

---

## 🏛️ Arquitetura

O projeto segue princípios da **Clean Architecture** e utiliza **MVC com Áreas (Areas)** para separar `EquipeMedix` e `UnidadeSaude`.

```mermaid
graph TD
    A[Apresentacao MVC] --> B[Aplicacao]
    AA[Apresentacao Area UnidadeSaude] --> B
    E[Apresentacao Web API] --> B
    D[Infraestrutura] --> B
    B --> C[Dominio]
````

---

### 🧩 Camadas

#### **Domínio (Domain)**

* Entidades: `UnidadeMedica`, `Paciente`, `Colaborador`
* Enums e Value Objects: `StatusUnidade`, `TipoColaborador`

#### **Aplicação (Application)**

* Casos de uso e lógica de negócio.
* ViewModels (ex.: `DashboardViewModel`, `CreateUnidadeViewModel`)
* DTOs (ex.: `UnidadeMedicaDto`, `LinkDto`)

#### **Infraestrutura (Infrastructure)**

* Repositórios com **Entity Framework Core (ApplicationDbContext)**
* Implementação do **ASP.NET Core Identity** com papéis (Roles)

#### **Apresentação (Presentation)**

* Projeto **ASP.NET Core MVC** (Equipe Medix)
* Área **UnidadeSaude**
* Controladores de API (`UnidadesMedicasApiController`)

---

## ✔️ Requisitos Funcionais (Sprint 2)

* **[RF-01]** Autenticação de Papéis
* **[RF-02]** Redirecionamento por Papel
* **[RF-03]** Criação de Utilizador Vinculado
* **[RF-04]** CRUD de Pacientes (Restrito)
* **[RF-05]** CRUD de Colaboradores (Restrito)
* **[RF-06]** API de Busca
* **[RF-07]** HATEOAS
* **[RF-08]** Busca no Front-End

---

## 🛠️ Tecnologias Utilizadas

### 🧩 Backend

* **.NET 8**
* **ASP.NET Core MVC (com Áreas)**
* **ASP.NET Core Web API**
* **Entity Framework Core 8**
* **ASP.NET Core Identity (com Papéis)**

### 🗄️ Banco de Dados

* **SQL Server (LocalDB para desenvolvimento)**

### 💻 Frontend

* HTML5, CSS3, JavaScript
* **Bootstrap 5**
* **Chart.js** (dashboards)
* **iMask.js** (máscaras de formulário)

### ⚙️ Ferramentas

* Visual Studio 2022
* Git & GitHub
* Postman (testes de API)

---

## 🚀 Como Executar o Projeto (Atualizado)

### ⚙️ Pré-requisitos

* .NET 8 SDK
* Visual Studio 2022 (com a carga de trabalho "ASP.NET MVC e desenvolvimento web")
* SQL Server Express LocalDB

---

### 🧭 Passo a passo

1.  **Clone o repositório**

    ```
    git clone https://github.com/challengeoracle/sprint-1-dotnet.git

    ```

2.  **Navegue até a pasta do projeto**

    ```
    cd sprint-1-dotnet

    ```

3.  **Abra o projeto no Visual Studio**

    -   Clique duas vezes no arquivo `.sln` para abrir a solução.

4.  **Restaure as dependências**

    -   O Visual Studio deve fazer isso automaticamente. Se necessário, execute no terminal:

    ```
    dotnet restore

    ```

5.  **Configure o Banco de Dados**

    -   No Visual Studio, abra o **Console do Gerenciador de Pacotes** (`View > Other Windows > Package Manager Console`).

    -   Execute o comando abaixo para criar o banco de dados e aplicar as *migrations*:

    ```
    Update-Database

    ```

6.  **Execute a Aplicação**

    -   No Visual Studio: pressione `F5` ou clique no botão de play.

    -   Ou pelo terminal:

    ```
    dotnet run

    ```

   * O `Program.cs` irá:

     * Executar as *migrations*
     * Criar o banco de dados
     * Criar os papéis `EquipeMedix` e `UnidadeSaude`
     * Criar o usuário admin padrão


---

### 🧾 Criar Acesso (Unidade de Saúde)

1. Faça login como admin
2. Vá em **Unidades Médicas → Adicionar Nova**
3. Preencha os campos da unidade, incluindo:

   * E-mail de acesso
   * Senha de acesso
4. Após salvar, saia e teste o login com os dados da nova unidade.

---
