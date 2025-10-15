Medix - Painel de Gestão de Unidades de Saúde
=============================================

### Painel administrativo interno para o gerenciamento completo do ciclo de vida de unidades de saúde parceiras.

> Projeto web desenvolvido com **ASP.NET Core MVC**, pensado como uma ferramenta de back office segura para a equipe interna da Medix administrar clínicas e hospitais parceiros: cadastro inicial, gerenciamento e permissões de acesso ao ecossistema principal da Medix.
>
> Desenvolvido para o Challenge FIAP em parceria com a Oracle.

👥 Integrantes do Grupo
-----------------------

-   **Arthur Thomas Mariano de Souza (RM 561061)** --- Responsável pelas matérias de IoT & IA Generativa, .NET e Mobile

-   **Davi Cavalcanti Jorge (RM 559873)** --- Responsável pelas matérias de Compliance & Q.A, DevOps e Mobile

-   **Mateus da Silveira Lima (RM 559728)** --- Responsável pelas matérias de Banco de Dados, Java e Mobile
  

🎯 Objetivo e Escopo
--------------------

O objetivo principal deste sistema é centralizar e simplificar a gestão de unidades de saúde parceiras. Através de um painel seguro, a equipe interna pode executar todo o ciclo de vida de um parceiro, desde o cadastro inicial e configuração de seus gestores até a desativação.

Isso garante controle de acesso rigoroso e organizado, assegurando que apenas entidades autorizadas e corretamente configuradas possam interagir com as APIs e serviços principais da Medix.


✨ Funcionalidades
-----------------

### 🏥 Gestão de Unidades de Saúde

-   [x] CRUD completo de unidades.

-   [x] Gerenciamento de status da unidade (Ativa, Inativa, Em Manutenção) com interface intuitiva.

### 🔐 Sistema de Autenticação e Autorização

-   [x] Login e registro para a equipe interna utilizando **ASP.NET Core Identity**.

-   [x] Contas individuais para auditoria e segurança aprimorada.

-   [x] Proteção de rotas --- apenas usuários autenticados acessam o painel.

### 🧭 Interface de Usuário

-   [x] Interface administrativa limpa, funcional e intuitiva.

-   [x] Localização em Português (Brasil).

-   [x] Design responsivo com **Bootstrap 5**.
        

🏛️ Arquitetura
---------------

O projeto segue princípios da **Clean Architecture**, buscando separação de responsabilidades, alta coesão, baixo acoplamento e testabilidade. A regra de dependência: camadas externas apontam para as internas.

``` mermaid
graph TD
    A[Presentation] --> B[Application]
    D[Infrastructure] --> B
    B --> C[Domain]

```

### 🧩 Camadas

#### Domínio (Domain)

-   Entidades (ex.: UnidadeMedica, ApplicationUser)

-   Enums e Value Objects (ex.: StatusUnidade)

-   Não depende de outras camadas.

#### Aplicação (Application)

-   Casos de uso (orquestra lógica).

-   Interfaces / contratos (ex.: IUnidadeMedicaRepository).

#### Infraestrutura (Infrastructure)

-   Implementação de repositórios com Entity Framework Core (ApplicationDbContext).

-   Implementação do ASP.NET Core Identity.

-   Integrações com serviços externos (e-mail, gateways, etc).

#### Apresentação (Presentation)

-   Projeto ASP.NET Core MVC com Controllers, Views e ViewModels.

-   Endpoints que expõem casos de uso para clientes externos.
  
  ---------------
### ✔️ Requisitos Funcionais

Os requisitos funcionais definem o que o sistema é capaz de fazer.

-   **[RF-01] Autenticação de Usuários:** O sistema deve permitir que os usuários se cadastrem, façam login e logout de forma segura.

-   **[RF-02] Gerenciamento de Conta:** Usuários autenticados devem poder gerenciar suas informações de conta, como alterar a senha.

-   **[RF-03] CRUD de Unidades Médicas:** O sistema deve permitir que usuários autorizados realizem as seguintes operações sobre as unidades médicas:

    -   **Criar:** Adicionar novas unidades médicas ao sistema.

    -   **Listar/Ler:** Visualizar uma lista de todas as unidades médicas cadastradas.

    -   **Detalhar:** Ver os detalhes completos de uma unidade médica específica.

    -   **Atualizar:** Editar as informações de uma unidade médica existente.

    -   **Excluir:** Remover uma unidade médica do sistema.

-   **[RF-04] Validação de Dados:** O sistema deve validar os dados inseridos nos formulários para garantir a integridade das informações.

---------------
### ❌ Requisitos Não Funcionais

Os requisitos não funcionais definem os critérios de qualidade e operação do sistema.

-   **[RNF-01] Plataforma e Tecnologia:** A aplicação deve ser construída utilizando o framework .NET 8 e ASP.NET Core MVC.

-   **[RNF-02] Acesso a Dados:** A persistência de dados deve ser gerenciada pelo Entity Framework Core, garantindo a abstração do banco de dados.

-   **[RNF-03] Segurança:**

    -   As senhas dos usuários devem ser armazenadas de forma segura utilizando hashing, conforme o padrão do ASP.NET Core Identity.

    -   O acesso às funcionalidades de gerenciamento de unidades médicas deve ser restrito a usuários autenticados.

-   **[RNF-04] Usabilidade:** A interface do usuário deve ser intuitiva e responsiva, utilizando o framework Bootstrap para se adaptar a diferentes tamanhos de tela.

-   **[RNF-05] Manutenibilidade:** O código deve estar organizado seguindo o padrão Model-View-Controller (MVC) para facilitar a manutenção e a evolução do projeto.


🛠️ Tecnologias Utilizadas
--------------------------

### Backend

-   **.NET 8**

-   **ASP.NET Core MVC**

-   **Entity Framework Core 8**

-   **ASP.NET Core Identity**

### Banco de Dados

-   **SQL Server** (LocalDB para desenvolvimento)

### Frontend

-   HTML5, CSS3, JavaScript

-   **Bootstrap 5**

### Ferramentas

-   Visual Studio 2022

-   Git & GitHub
  

🚀 Como Executar o Projeto
--------------------------

### ⚙️ Pré-requisitos

-   .NET 8 SDK

-   Visual Studio 2022 (com a carga de trabalho "ASP.NET MVC e desenvolvimento web")

-   SQL Server Express LocalDB (geralmente instalado com o Visual Studio)

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

7.  **Primeiro Acesso**

    -   A aplicação estará disponível em `https://localhost:xxxx` (a porta pode variar).

    -   Use a opção "Registrar" na página de login para criar sua primeira conta de administrador.
