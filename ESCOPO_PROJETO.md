# RPG Workspace API - Escopo do Projeto

## Visao geral

O projeto `RpgWorkspace` e uma API backend para gerenciamento colaborativo de campanhas de RPG. A aplicacao permite que usuarios se cadastrem, autentiquem, criem workspaces, organizem campanhas e mantenham informacoes de apoio para a mesa, como sessoes, NPCs, locais e quests.

A solucao foi estruturada em camadas, seguindo uma separacao clara entre API, regras de aplicacao, dominio e infraestrutura. O backend utiliza .NET 8, ASP.NET Core, Entity Framework Core, PostgreSQL, autenticacao JWT e documentacao via Swagger.

## Objetivo do produto

Centralizar a organizacao de campanhas de RPG em um ambiente compartilhado, permitindo que mestres e jogadores acessem informacoes relevantes da campanha conforme suas permissoes.

O sistema prioriza:

- Organizacao de campanhas por workspace.
- Controle de acesso por usuario autenticado.
- Diferenciacao de papeis dentro de um workspace.
- Registro de elementos narrativos da campanha.
- Separacao entre conteudo publico para jogadores e conteudo privado para mestres.

## Stack tecnica

- Linguagem: C#
- Plataforma: .NET 8
- Framework web: ASP.NET Core Web API
- ORM: Entity Framework Core
- Banco de dados: PostgreSQL
- Autenticacao: JWT Bearer
- Hash de senha: BCrypt
- Documentacao da API: Swagger / Swashbuckle
- Health checks: ASP.NET Core HealthChecks com verificacao PostgreSQL

## Estrutura da solucao

```text
RpgWorkspace.sln
src/
  RpgWorkspace.Api/
  RpgWorkspace.Application/
  RpgWorkspace.Domain/
  RpgWorkspace.Infrastructure/
```

### RpgWorkspace.Api

Camada de entrada HTTP da aplicacao.

Responsabilidades:

- Definir controllers REST.
- Configurar pipeline ASP.NET Core.
- Habilitar autenticacao e autorizacao.
- Expor Swagger em ambiente de desenvolvimento.
- Expor endpoints de health check.
- Tratar excecoes via middleware.

Controllers encontrados:

- `AuthController`
- `WorkspacesController`
- `CampaignsController`
- `SessionsController`
- `NpcsController`
- `LocationsController`
- `QuestsController`
- `HealthController`

### RpgWorkspace.Application

Camada de casos de uso e contratos da aplicacao.

Responsabilidades:

- Implementar servicos de aplicacao.
- Definir interfaces de servicos, repositorios, token, senha e unidade de trabalho.
- Definir DTOs de entrada e saida.
- Aplicar regras de acesso e orquestrar operacoes de dominio.

Servicos encontrados:

- `AuthService`
- `WorkspaceService`
- `CampaignService`
- `SessionService`
- `NpcService`
- `LocationService`
- `QuestService`

### RpgWorkspace.Domain

Camada de dominio do sistema.

Responsabilidades:

- Definir entidades principais.
- Definir enums de status, tipo, importancia e papel.
- Concentrar comportamentos simples das entidades, como criacao e atualizacao.

Entidades encontradas:

- `User`
- `Workspace`
- `WorkspaceMember`
- `Campaign`
- `Session`
- `Npc`
- `Location`
- `Quest`

### RpgWorkspace.Infrastructure

Camada de infraestrutura e persistencia.

Responsabilidades:

- Configurar Entity Framework Core.
- Implementar repositorios.
- Implementar `UnitOfWork`.
- Implementar geracao de JWT.
- Implementar hash e verificacao de senha com BCrypt.
- Registrar dependencias da aplicacao.
- Manter migrations do banco.

## Modelo de dominio

### User

Representa um usuario da plataforma.

Campos principais:

- `Name`
- `Email`
- `PasswordHash`

Regras observadas:

- E-mail e normalizado para minusculo.
- Senha e armazenada como hash.
- Login retorna token JWT quando as credenciais sao validas.

### Workspace

Representa um espaco colaborativo onde campanhas sao organizadas.

Campos principais:

- `Name`
- `Description`
- `OwnerUserId`
- `Members`
- `Campaigns`

Regras observadas:

- Ao criar um workspace, o usuario criador se torna `Owner`.
- Apenas membros conseguem visualizar o workspace.
- Apenas o `Owner` pode atualizar ou excluir o workspace.

### WorkspaceMember

Relaciona usuarios a workspaces.

Campos principais:

- `WorkspaceId`
- `UserId`
- `Role`

Papeis disponiveis:

- `Owner`
- `Master`
- `Player`

### Campaign

Representa uma campanha de RPG dentro de um workspace.

Campos principais:

- `WorkspaceId`
- `Name`
- `Description`
- `SystemName`
- `Sessions`
- `Npcs`
- `Locations`
- `Quests`

Regras observadas:

- Membros do workspace podem listar e visualizar campanhas.
- Apenas `Owner` ou `Master` podem criar e atualizar campanhas.
- Apenas `Owner` pode excluir campanhas.

### Session

Representa uma sessao da campanha.

Campos principais:

- `CampaignId`
- `Title`
- `Number`
- `Date`
- `Summary`
- `Notes`
- `Status`

Status disponiveis:

- `Planned`
- `Completed`
- `Canceled`

Regras observadas:

- Membros podem listar e visualizar sessoes.
- Apenas `Owner` ou `Master` podem criar, atualizar ou excluir sessoes.

### Npc

Representa um personagem nao jogavel da campanha.

Campos principais:

- `CampaignId`
- `Name`
- `Description`
- `Status`
- `IsPrivate`
- `Notes`

Status disponiveis:

- `Alive`
- `Dead`
- `Missing`
- `Unknown`

Regras observadas:

- NPCs privados so aparecem para `Owner` ou `Master`.
- Membros podem ver NPCs publicos.
- Apenas `Owner` ou `Master` podem criar, atualizar ou excluir NPCs.

### Location

Representa um local relevante da campanha.

Campos principais:

- `CampaignId`
- `Name`
- `Type`
- `Description`
- `Region`
- `Importance`
- `IsPrivate`

Tipos disponiveis:

- `City`
- `Kingdom`
- `Dungeon`
- `School`
- `Tavern`
- `Region`
- `Forest`
- `Temple`
- `Other`

Niveis de importancia:

- `Low`
- `Medium`
- `High`
- `Critical`

Regras observadas:

- Locais privados so aparecem para `Owner` ou `Master`.
- Membros podem ver locais publicos.
- Apenas `Owner` ou `Master` podem criar, atualizar ou excluir locais.

### Quest

Representa uma missao, objetivo ou gancho narrativo da campanha.

Campos principais:

- `CampaignId`
- `Title`
- `Description`
- `Status`
- `Reward`
- `IsPrivate`

Status disponiveis:

- `NotStarted`
- `InProgress`
- `Completed`
- `Failed`
- `Abandoned`

Regras observadas:

- Quests privadas so aparecem para `Owner` ou `Master`.
- Membros podem ver quests publicas.
- Apenas `Owner` ou `Master` podem criar, atualizar ou excluir quests.

## Autenticacao e autorizacao

A API utiliza JWT Bearer.

Fluxo principal:

1. Usuario registra uma conta em `/api/auth/register`.
2. Usuario faz login em `/api/auth/login`.
3. API retorna um token JWT.
4. Endpoints protegidos exigem header `Authorization: Bearer {token}`.

As regras de autorizacao por recurso sao aplicadas nos servicos de aplicacao, usando o `userId` extraido das claims do token.

## Endpoints principais

### Autenticacao

- `POST /api/auth/register`
- `POST /api/auth/login`

### Workspaces

- `GET /api/workspaces`
- `GET /api/workspaces/{id}`
- `POST /api/workspaces`
- `PUT /api/workspaces/{id}`
- `DELETE /api/workspaces/{id}`

### Campaigns

- `GET /api/workspaces/{workspaceId}/campaigns`
- `GET /api/campaigns/{id}`
- `POST /api/workspaces/{workspaceId}/campaigns`
- `PUT /api/campaigns/{id}`
- `DELETE /api/campaigns/{id}`

### Sessions

- `GET /api/campaigns/{campaignId}/sessions`
- `GET /api/sessions/{id}`
- `POST /api/campaigns/{campaignId}/sessions`
- `PUT /api/sessions/{id}`
- `DELETE /api/sessions/{id}`

### NPCs

- `GET /api/campaigns/{campaignId}/npcs`
- `GET /api/npcs/{id}`
- `POST /api/campaigns/{campaignId}/npcs`
- `PUT /api/npcs/{id}`
- `DELETE /api/npcs/{id}`

### Locations

- `GET /api/campaigns/{campaignId}/locations`
- `GET /api/locations/{id}`
- `POST /api/campaigns/{campaignId}/locations`
- `PUT /api/locations/{id}`
- `DELETE /api/locations/{id}`

### Quests

- `GET /api/campaigns/{campaignId}/quests`
- `GET /api/quests/{id}`
- `POST /api/campaigns/{campaignId}/quests`
- `PUT /api/quests/{id}`
- `DELETE /api/quests/{id}`

### Health checks

- `GET /health`
- `GET /health/live`
- `GET /health/ready`

## Persistencia

A persistencia usa Entity Framework Core com PostgreSQL.

O `AppDbContext` expoe os seguintes `DbSet`:

- `Users`
- `Workspaces`
- `WorkspaceMembers`
- `Campaigns`
- `Sessions`
- `Npcs`
- `Locations`
- `Quests`

As configuracoes de mapeamento ficam em:

```text
src/RpgWorkspace.Infrastructure/Persistence/Configurations/
```

As migrations existentes indicam evolucao incremental dos modulos:

- Criacao inicial
- Modulo de workspace
- Modulo de campanha
- Modulo de sessao
- Modulo de NPC
- Modulo de locais
- Modulo de quests

## Configuracao

Configuracao principal em:

```text
src/RpgWorkspace.Api/appsettings.json
```

Chaves relevantes:

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:Secret`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `JwtSettings:ExpirationMinutes`

Observacao: o segredo JWT e a senha do banco aparecem como valores de desenvolvimento e devem ser substituidos por variaveis de ambiente ou secrets em producao.

## Regras de acesso resumidas

| Recurso | Listar/visualizar | Criar | Atualizar | Excluir |
| --- | --- | --- | --- | --- |
| Workspace | Membro | Usuario autenticado | Owner | Owner |
| Campaign | Membro do workspace | Owner/Master | Owner/Master | Owner |
| Session | Membro do workspace | Owner/Master | Owner/Master | Owner/Master |
| NPC publico | Membro do workspace | Owner/Master | Owner/Master | Owner/Master |
| NPC privado | Owner/Master | Owner/Master | Owner/Master | Owner/Master |
| Location publica | Membro do workspace | Owner/Master | Owner/Master | Owner/Master |
| Location privada | Owner/Master | Owner/Master | Owner/Master | Owner/Master |
| Quest publica | Membro do workspace | Owner/Master | Owner/Master | Owner/Master |
| Quest privada | Owner/Master | Owner/Master | Owner/Master | Owner/Master |

## Fora do escopo atual observado

Durante a analise do codigo, nao foram encontrados recursos implementados para:

- Convite ou gerenciamento completo de membros de workspace.
- Alteracao de papel de membros.
- Recuperacao ou troca de senha.
- Perfil completo do usuario.
- Upload de imagens, mapas ou anexos.
- Sistema de personagens jogaveis.
- Diario de campanha ou timeline consolidada.
- Comentarios, notificacoes ou atividade em tempo real.
- Testes automatizados.
- Frontend ou cliente web/mobile.

## Possiveis proximos passos

- Criar endpoints para convidar membros ao workspace.
- Permitir gerenciamento de papeis (`Owner`, `Master`, `Player`).
- Adicionar modulo de personagens jogaveis.
- Adicionar testes unitarios para servicos de aplicacao.
- Adicionar testes de integracao para controllers e regras de autorizacao.
- Mover secrets de desenvolvimento para variaveis de ambiente.
- Corrigir ou atualizar o arquivo `.http`, que ainda referencia `/weatherforecast/`.
- Revisar textos com caracteres corrompidos em comentarios do codigo, provavelmente causados por encoding.
- Criar documentacao de execucao local com comandos para banco, migrations e start da API.

## Resumo executivo

O projeto atual e um backend bem direcionado para organizar campanhas de RPG com controle de acesso por workspace. A base ja cobre autenticacao, workspaces, campanhas e quatro modulos centrais de apoio ao mestre: sessoes, NPCs, locais e quests. A arquitetura em camadas esta adequada para evolucao do produto, especialmente para adicionar novos modulos sem misturar regras de negocio com detalhes de persistencia ou HTTP.
