<div align="center">

  <img src="./logo/logo-horizontal.png" alt="Day Mendes Store" width="440" />

  <p><strong>Sistema de gestão comercial, controle de estoque por variações físicas e emissão de relatórios executivos para moda feminina.</strong></p>

  <p>
    <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET_10-9D3556?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 10" /></a>
    <a href="https://learn.microsoft.com/dotnet/csharp/"><img src="https://img.shields.io/badge/C%23_14-9D3556?style=flat-square&logo=csharp&logoColor=white" alt="C#" /></a>
    <a href="https://learn.microsoft.com/aspnet/core/"><img src="https://img.shields.io/badge/ASP.NET_Core-9D3556?style=flat-square&logo=dotnet&logoColor=white" alt="ASP.NET Core" /></a>
    <a href="https://vuejs.org/"><img src="https://img.shields.io/badge/Vue.js_3-9D3556?style=flat-square&logo=vuedotjs&logoColor=white" alt="Vue 3" /></a>
    <a href="https://www.typescriptlang.org/"><img src="https://img.shields.io/badge/TypeScript-9D3556?style=flat-square&logo=typescript&logoColor=white" alt="TypeScript" /></a>
    <a href="https://www.mysql.com/"><img src="https://img.shields.io/badge/MySQL_8.0-9D3556?style=flat-square&logo=mysql&logoColor=white" alt="MySQL" /></a>
    <a href="https://learn.microsoft.com/ef/core/"><img src="https://img.shields.io/badge/EF_Core_9-9D3556?style=flat-square&logo=dotnet&logoColor=white" alt="EF Core" /></a>
    <a href="https://www.questpdf.com/"><img src="https://img.shields.io/badge/QuestPDF-9D3556?style=flat-square" alt="QuestPDF" /></a>
  </p>

</div>

---

## Sobre o Projeto

O **Day Mendes Store** é uma plataforma de gestão desenvolvida especificamente para as necessidades operacionais e estratégicas do varejo de moda feminina. O sistema unifica o controle de catálogo, inventário com múltiplos atributos, cadastro de clientes, frente de caixa (PDV) e relatórios de desempenho comercial.

A arquitetura foi projetada para resolver a complexidade inerente ao setor de vestuário: o controle de estoque fracionado por variações físicas de **tamanho** e **cor**, com garantia de consistência transacional em vendas, devoluções e reposições.

---

## Objetivo

Fornecer uma solução centralizada, confiável e intuitiva para a gestão diária da loja, eliminando divergências de inventário, acelerando o fechamento de pedidos no caixa e gerando inteligência comercial para compras de reposição no atacado.

---

## Principais Funcionalidades

### Gestão de Produtos e Categorias
* Cadastro detalhado de produtos com nome, marca, categoria, preço de custo, preço de venda e limite de estoque mínimo.
* Visualização flexível do catálogo em formato de cards minimalistas ou tabela densa.
* Busca textual dinâmica em tempo real por nome, código ou marca.
* Exclusão segura com suporte a exclusão lógica (*soft delete*) e inativação quando houver histórico de vendas vinculado.

### Variações Físicas e Controle de Estoque
* Estoque atomizado por combinação de tamanho (P, M, G, GG, Único) e cor.
* Saldo total do produto calculado dinamicamente com base nas variações ativas.
* Modal de visualização e edição direta de quantidades por variação sem necessidade de abrir formulários complexos.
* Alertas automáticos para produtos com estoque zerado ou abaixo do limite de segurança.

### Geração de Etiquetas Térmicas e Código de Barras
* Geração vetorial (SVG) de códigos de barras no padrão Code 39, eliminando dependência de serviços externos.
* Layout otimizado para impressão em bobinas térmicas e etiquetas adesivas para confecção.
* Impressão individual por produto ou em lote por seleção múltipla no catálogo.
* Identificação visual com nome da loja, produto, marca, tamanho, cor, código e valor de venda.

### Frente de Caixa (PDV)
* Lançamento ágil de vendas com seleção de produto e variação física.
* Associação com cliente cadastrado ou venda anônima para consumidor final.
* Aplicação de descontos ou acréscimos monetários no fechamento.
* Múltiplos métodos de pagamento (Dinheiro, PIX, Cartão de Crédito, Cartão de Débito).
* Cálculo automático de troco e impressão de comprovante não fiscal.
* Salvamento de pedidos em rascunho para retomada posterior.

### Histórico de Vendas e Cancelamento com Estorno
* Painel de consulta de vendas com filtros por status (Finalizada, Rascunho, Cancelada).
* Detalhamento de itens, valores, formas de pagamento e dados do cliente.
* Cancelamento de venda com estorno atômico de estoque, retornando as peças automaticamente para a variação física de origem.

### Gestão de Clientes e Histórico
* Cadastro e edição com nome, apelido, telefone, e-mail e endereço.
* Modal de histórico consolidado por cliente com métricas de total comprado, quantidade de compras, ticket médio e lista detalhada de pedidos anteriores.

### Relatórios Estratégicos e Exportação em PDF
* Filtros combinados por período (data inicial e final), categoria e grade de tamanho.
* Indicadores gerais de desempenho: total faturado, lucro bruto estimado, quantidade de peças vendidas e ticket médio.
* Distribuição de faturamento por método de pagamento e categorias mais rentáveis.
* Desempenho de produtos: peças mais vendidas, produtos com estoque crítico e peças paradas sem giro há mais de 30 dias.
* Sugestões de compra para fornecedores (Brás), com cálculo automático de quantidade sugerida e justificativa baseada no histórico de vendas.
* Análise de velocidade de saída por tamanho e ranking de melhores clientes.
* Exportação de relatório executivo diagramado em PDF via **QuestPDF**, formatado com a identidade visual da loja.

### Upload e Armazenamento de Imagens
* Endpoint dedicado multipart/form-data para upload de fotos de produtos.
* Armazenamento estático estruturado no servidor com geração de identificadores únicos (GUID).
* Suporte a imagens locais e URLs externas com fallback visual para produtos sem foto.

### Autenticação e Segurança
* Autenticação stateless via JSON Web Token (JWT) com Bearer Scheme.
* Criptografia unidirecional de senhas com algoritmo BCrypt.
* Rota de configuração e primeiro acesso da loja.

---

## Módulos do Sistema

O sistema é dividido nos seguintes módulos principais na interface:

1. **Painel**: Resumo dos principais indicadores diários da loja.
2. **Produtos**: Catálogo completo, gestão de categorias, estoque por variação, etiquetas e controle de produtos.
3. **Clientes**: Base de contatos, histórico de compras individuais e métricas de relacionamento.
4. **PDV**: Frente de caixa para montagem de pedidos, pagamento e emissão de comprovantes.
5. **Vendas**: Acompanhamento de pedidos, consulta detalhada e operações de estorno/cancelamento.
6. **Relatórios**: Visão geral financeira, análise de desempenho de peças, sugestões de compra para fornecedores e exportação em PDF.

---

## Modelagem de Produto e Variação

Para refletir a realidade do varejo de moda, o catálogo opera sob uma relação de 1 para N entre o modelo e suas unidades físicas:

* **Produto**: Entidade conceitual do modelo (nome, marca, categoria, valor de compra, valor de venda e estoque mínimo).
* **VariacaoProduto**: Entidade física onde reside o saldo real de estoque (combinação de tamanho e cor).

```text
Camisa Linho Alfaiataria (Produto)
├── P / Off-White  →  4 unidades (Variação)
├── M / Off-White  →  6 unidades (Variação)
├── M / Terracota  →  3 unidades (Variação)
└── G / Terracota  →  2 unidades (Variação)
```

O estoque exibido no catálogo é a consolidação dinâmica das quantidades das variações ativas.

---

## Arquitetura e Estrutura

A solução adota separação em camadas inspirada na Clean Architecture:

```text
day-mendes-store/
├── backend/
│   ├── DayMendesStore.API/            # Controllers REST, autenticação JWT, upload e middlewares
│   ├── DayMendesStore.Application/    # Casos de uso, interfaces, DTOs e serviços (QuestPDF, relatórios)
│   ├── DayMendesStore.Domain/         # Entidades puras, enums de status e regras de domínio
│   ├── DayMendesStore.Infrastructure/ # DbContext, mapeamentos EF Core, migrações e repositórios
│   └── DayMendesStore.slnx            # Arquivo de solução .NET
│
├── frontend/                          # Aplicação SPA modular (Vue 3, TypeScript e Vite)
│   ├── src/
│   │   ├── components/                # Componentes organizados por domínio (produtos, pdv, vendas, clientes)
│   │   ├── pages/                     # Telas de login e fluxos principais
│   │   ├── api.ts                     # Camada de comunicação HTTP tipada
│   │   └── App.vue                    # Shell principal da aplicação com navegação responsiva
│
└── logo/                              # Identidade visual da marca Day Mendes Store
```

---

## Tecnologias Utilizadas

### Backend
* **C# 14 / .NET 10**: Runtime e framework da API REST (ASP.NET Core).
* **Entity Framework Core 9**: ORM para persistência e mapeamento objeto-relacional.
* **Pomelo.EntityFrameworkCore.MySql**: Driver otimizado para integração com MySQL Server.
* **MySQL 8.0**: Banco de dados relacional.
* **QuestPDF (2026.8.0)**: Biblioteca para diagramação e renderização de relatórios em PDF.
* **BCrypt.Net-Next**: Hashing seguro e verificação de senhas.
* **System.IdentityModel.Tokens.Jwt**: Emissão e validação de tokens JWT.
* **Swagger / Swashbuckle**: Documentação interativa e testes de endpoints.

### Frontend
* **Vue.js 3**: Framework progressivo utilizando Composition API e sintaxe `<script setup>`.
* **TypeScript**: Tipagem estrita de contratos de dados e componentes.
* **Vite 8**: Ferramenta de build rápido e servidor de desenvolvimento.
* **CSS Nativo**: Estilização moderna, responsiva e alinhada à paleta da marca (Marsala e neutros quentes).

---

## Como Executar Localmente

### Execução com Docker (recomendado)

1. Copie `.env.example` para `.env` e defina senhas seguras, principalmente
   `JWT_SECRET_KEY`, `DOCKER_DB_PASSWORD` e `DB_ROOT_PASSWORD`.
2. Na raiz do projeto, execute:

   ```bash
   docker compose up --build -d
   ```

3. Acesse a aplicação em `http://localhost:8080`. A API também fica exposta em
   `http://localhost:5114` para acesso direto.

As migrations são aplicadas automaticamente quando a API inicia. Os dados do
MySQL e as imagens enviadas ficam persistidos nos volumes `mysql_data` e
`uploads_data`.

Para acompanhar os serviços, use `docker compose logs -f`. Para encerrá-los,
use `docker compose down` (sem `-v`, para preservar os dados).

#### Após clonar o projeto no Windows (PowerShell)

Execute na raiz do projeto:

```powershell
(Get-Content .env.example) -replace '^JWT_SECRET_KEY=.*$', ('JWT_SECRET_KEY=' + [guid]::NewGuid().ToString('N')) | Set-Content .env
docker compose up -d
```

A aplicação estará em `http://localhost:8080`. Para visualizar o banco no
DBeaver ou MySQL Workbench, use:

```text
Host: localhost
Porta: 3306
Banco: day_mendes_store
Usuário: daymendes
Senha: daymendes_local
```

Essas credenciais são apenas para execução local. Não exponha a porta 3306 nem
use essas senhas em um servidor público.

---

### Pré-requisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) instalado.
* [Node.js](https://nodejs.org/) (versão 22 ou superior) instalado.
* [MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/) em execução.

---

### 1. Configuração e Execução do Backend

1. Navegue até o diretório do backend:
   ```bash
   cd backend
   ```

2. Configure a conexão com o banco de dados no arquivo `DayMendesStore.API/appsettings.json` ou defina as variáveis de ambiente equivalentes:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Port=3306;Database=day_mendes_store;User=root;Password=SUA_SENHA_AQUI;"
   }
   ```

3. Aplique as migrações do Entity Framework para criar as tabelas no MySQL:
   ```bash
   dotnet ef database update --project DayMendesStore.Infrastructure --startup-project DayMendesStore.API
   ```

4. Execute o servidor da API:
   ```bash
   dotnet run --project DayMendesStore.API
   ```

A API estará disponível em `http://localhost:5114`.

A documentação Swagger interativa poderá ser acessada em `http://localhost:5114/swagger`.

---

### 2. Configuração e Execução do Frontend

1. Em outro terminal, navegue até o diretório do frontend:
   ```bash
   cd frontend
   ```

2. Instale as dependências:
   ```bash
   npm install
   ```

3. Inicie o servidor de desenvolvimento:
   ```bash
   npm run dev
   ```

A interface web estará acessível em `http://localhost:5173`.

---

## Qualidade e Validação

* **Tipagem Estrita**: O frontend possui verificação de tipos completa através do `vue-tsc --build`, garantindo conformidade entre as respostas da API e os componentes visuais.
* **Validação de Build**: Processo de build de produção verificado via `npm run build` (Vite + `vue-tsc`).
* **Documentação de Endpoints**: Todas as rotas REST são mapeadas e testáveis diretamente pelo Swagger UI com suporte à autenticação Bearer JWT.
* **Testes Automatizados**: A versão atual do projeto foca na validação manual integrada e testes pontuais via Swagger; suítes de testes de unidade automatizados (xUnit/Vitest) estão planejadas para os próximos ciclos de desenvolvimento.

---

<div align="center">
  <p><strong>Day Mendes Store</strong> &bull; Sistema de Gestão Comercial &bull; 2026</p>
</div>
