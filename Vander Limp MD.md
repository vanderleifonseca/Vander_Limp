# ESPECIFICAÇÃO TÉCNICA E ARQUITETURA DE SOFTWARE
**Projeto:** Sistema de Força de Vendas e Pedidos (Produtos de Limpeza)  
**Stack Tecnológica:** .NET MAUI (C#), ASP.NET Core Web API, PostgreSQL, SQLite Local.  
**Data:** Outubro de 2026  

---

## 1. ESPECIFICAÇÕES GERAIS DO SISTEMA

### 1.1. Objetivos do Sistema
* Permitir a força de vendas e autoatendimento B2B e B2C para a comercialização de produtos de limpeza.
* Oferecer navegação **Offline-First**, garantindo a persistência de dados local no dispositivo móvel.
* Agilizar a rotina de compras recorrentes permitindo a **repetição automática do último pedido**.
* Garantir a sincronização transparente de catálogo, cadastros e pedidos através de um Web Service RESTful.

### 1.2. Atores do Sistema
* **Cliente (Usuário Móvel):** Realiza o cadastro inicial (Pessoa Física ou Jurídica), visualiza o catálogo sincronizado, monta e envia pedidos, além de acompanhar os status de entrega.
* **Empresa / Retaguarda (Administrativo):** Recebe os cadastros e pedidos via API central, gerencia a disponibilidade e status dos produtos (`Ativo` / `Inativo`) e atualiza a situação da expedição/despacho do pedido.

---

## 2. REQUISITOS DO SISTEMA

### 2.1. Regras de Negócio (RN)

| Código | Descrição da Regra de Negócio |
| :--- | :--- |
| **RN-01** | **Auto-login e Identificação Única:** No primeiro acesso do app, se não houver cadastro salvo localmente, o sistema força a exibição da Tela de Cadastro. Em acessos posteriores, se o cadastro existir e estiver autorizado, o app abre direto na tela principal de Pedidos. |
| **RN-02** | **Cadastro Unificado (PF/PJ):** O sistema deve aceitar cadastros tanto de Pessoa Física (CPF) quanto de Pessoa Jurídica (CNPJ). |
| **RN-03** | **Filtro de Exibição de Produtos:** Somente produtos com a flag `ativo = true` no banco de dados serão retornados pela API e exibidos no aplicativo. |
| **RN-04** | **Quantidade Padrão no Pedido:** Ao selecionar um produto para incluir no pedido, a quantidade inicial pré-preenchida deve ser igual a `1`. |
| **RN-05** | **Repetição do Último Pedido:** Ao optar por repetir o pedido anterior, o app deve carregar automaticamente os mesmos itens e quantidades do último pedido registrado. |
| **RN-06** | **Persistência Dupla de Pedidos:** Todo pedido finalizado deve ser salvo no SQLite local do dispositivo móvel e enviado para o PostgreSQL no servidor via API Web Service. |

---

### 2.2. Dicionário de Dados e Atributos

#### A. Entidade: Cliente
* `Id` (UUID / Integer - Chave Primária)
* `TipoPessoa` (Enum: `PF`, `PJ`)
* `CPF_CNPJ` (String - Obrigatório, Único)
* `NomeRazaoSocial` (String - Obrigatório)
* `EnderecoCompleto` (String - Obrigatório)
* `Telefone` (String)
* `Email` (String)
* `StatusAutorizado` (Boolean - Padrão: `True`)

#### B. Entidade: Produto
* `Id` (UUID / Integer - Chave Primária)
* `Codigo` (String / Integer - Código interno de identificação)
* `Descricao` (String - Ex: "Detergente Neutro")
* `Marca` (String - Ex: "LimpaMais")
* `UnidadeMedida` (Enum / String: `ML`, `Litros`, `Kg`, `Sachê`, `Pacote`, `Unidade`)
* `PrecoUnitario` (Decimal)
* `Ativo` (Boolean - Flag de controle de exibição)

#### C. Entidade: Pedido
* `Id` (UUID / Integer - Chave Primária)
* `ClienteId` (Chave Estrangeira -> Cliente)
* `FornecedorId` (Integer / String)
* `FormaPagamento` (Enum: `Cartao`, `Dinheiro`, `Pix`)
* `ValorTotal` (Decimal)
* `Status` (Enum: `EmSeparacao`, `Enviado`, `Entregue`)
* `DataCriacao` (DateTime)

#### D. Entidade: ItemPedido
* `Id` (UUID / Integer - Chave Primária)
* `PedidoId` (Chave Estrangeira -> Pedido)
* `ProdutoId` (Chave Estrangeira -> Produto)
* `Quantidade` (Decimal / Integer)
* `PrecoUnitario` (Decimal)
* `Subtotal` (Decimal)

---

### 2.3. Fluxos Operacionais

#### Fluxo 1: Inicialização e Cadastro (Primeiro Acesso)
1. O usuário abre o aplicativo .NET MAUI.
2. O app consulta a base local (SQLite) verificando a existência de um perfil salvo.
3. **Se não existir:** Redireciona para a **Tela de Cadastro (PF/PJ)**.
4. O cliente preenche seus dados (Nome/Razão Social, CPF/CNPJ, Endereço, etc.) e clica em **"Salvar"**.
5. O app grava os dados localmente no SQLite e dispara a solicitação para o endpoint `POST /api/v1/clientes`.
6. Após a confirmação, o aplicativo sincroniza a lista de produtos atualizada e redireciona para a tela inicial de Pedidos.

#### Fluxo 2: Emissão de Novo Pedido
1. O usuário abre o app. Como já possui cadastro local, navega diretamente para a **Tela de Pedidos**.
2. Clica no botão **"Novo Pedido"**.
3. Seleciona o **Fornecedor**.
4. É exibida a lista de produtos ativos (exibindo Código, Marca, Descrição, Unidade e Preço).
5. O usuário escolhe os produtos e ajusta as quantidades desejadas (iniciando em `1`).
6. Seleciona a Forma de Pagamento (`Cartão`, `Dinheiro` ou `PIX`).
7. Clica em **"Finalizar Pedido"**.
8. O pedido é salvo no banco SQLite local e enviado via API (`POST /api/v1/pedidos`) para o servidor PostgreSQL.

#### Fluxo 3: Repetição do Último Pedido
1. Na tela principal, o usuário clica na opção **"Repetir Último Pedido"**.
2. O app busca localmente ou via API o último pedido concluído do cliente.
3. O carrinho é preenchido automaticamente com os mesmos produtos e quantidades.
4. O usuário confirma os dados de pagamento e finaliza o pedido de forma rápida.

---

## 3. FERRAMENTAS E TECNOLOGIAS

### 3.1. Aplicativo Mobile
* **Framework Principal:** .NET MAUI (C#) — Solução Multiplataforma (Android / iOS).
* **Padrão de Arquitetura:** MVVM (Model-View-ViewModel) com suporte da biblioteca `CommunityToolkit.Mvvm`.
* **Banco de Dados Local:** SQLite via `Entity Framework Core SQLite` ou `SQLite-net-pcl`.
* **Comunicação de Rede:** `HttpClient` / `Refit` para integração com serviços RESTful.

### 3.2. Backend / Web Service
* **Framework de API:** .NET 8 / ASP.NET Core Web API (C#).
* **ORM (Mapeamento Objeto-Relacional):** Entity Framework Core.
* **Banco de Dados Servidor:** PostgreSQL.

### 3.3. Ambiente de Desenvolvimento
* Visual Studio 2022 ou JetBrains Rider.
* Docker (containerização do PostgreSQL em ambiente de desenvolvimento).
* Postman / Insomnia (validação dos endpoints da API REST).

---

## 4. ESTRUTURA DE IMPLEMENTAÇÃO E ARQUITETURA

### 4.1. Arquitetura da Solução em C# (.NET)

```text
MeuSistemaVendas/
├── src/
│   ├── Client.Maui/               # Projeto Mobile .NET MAUI
│   │   ├── Views/                 # Telas (CadastroPage, PedidosListPage, NovoPedidoPage)
│   │   ├── ViewModels/            # Lógica de Apresentação e Estado
│   │   ├── Services/              # Sincronização, Consumo da API e SQLite Local
│   │   └── Models/                # Entidades Locais e DTOs
│   │
│   ├── Server.API/                # Projeto Web API ASP.NET Core
│   │   ├── Controllers/           # Endpoints (ClientesController, ProdutosController, PedidosController)
│   │   ├── Services/              # Regras e Serviços de Negócio
│   │   ├── Infrastructure/        # DbContext (EF Core) e Repositórios PostgreSQL
│   │   └── Domain/                # Entidades do Domínio Principal