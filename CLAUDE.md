# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

`RpgWorkspace` is a .NET 8 ASP.NET Core Web API for collaboratively managing tabletop RPG campaigns. Multiple users share a `Workspace`, organize `Campaign`s inside it, and each campaign holds both GM-facing content (sessions, NPCs, locations, quests, wiki pages, world-library/rules entries) and per-player content tied to a `Character` (player notes, theories, operations, important people, narrative items — an investigation/journal system for a player's character).

There is a companion `ESCOPO_PROJETO.md` in this folder with a Portuguese write-up of the domain — it documents the original 8-entity slice of the project (User/Workspace/Campaign/Session/Npc/Location/Quest/WorkspaceMember) and is **out of date**: the codebase has since grown to ~20 entities and 20 controllers (Characters, Dashboard, ImportantPeople, NarrativeItems, Operations, PlayerNotes, Schedule, Search, Tags, Theories, WikiPages, WorkspaceInvites, WorkspaceMembers, WorldLibrary). Prefer reading the code over that doc for anything beyond the original core modules.

## Commands

All commands assume the working directory is `Back/` (where `RpgWorkspace.sln` lives).

```bash
# restore & build the whole solution
dotnet restore
dotnet build RpgWorkspace.sln

# run the API (uses src/RpgWorkspace.Api/Properties/launchSettings.json)
dotnet run --project src/RpgWorkspace.Api            # http://localhost:5069, https://localhost:7019
dotnet watch --project src/RpgWorkspace.Api           # hot reload

# EF Core migrations (Infrastructure holds DbContext/migrations, Api is the startup project)
dotnet ef migrations add <Name> --project src/RpgWorkspace.Infrastructure --startup-project src/RpgWorkspace.Api
dotnet ef database update --project src/RpgWorkspace.Infrastructure --startup-project src/RpgWorkspace.Api
```

- There is **no test project** in the solution yet (`RpgWorkspace.sln` only lists Domain/Application/Infrastructure/Api). If you add tests, wire a new project into the `.sln` and this section.
- Swagger UI is only enabled in `Development` (`Program.cs` gates `UseSwaggerWithUi()` behind `IsDevelopment()`), reachable at the API root once running.
- Requires a local PostgreSQL instance matching `ConnectionStrings:DefaultConnection` in `src/RpgWorkspace.Api/appsettings.json` (prod-shaped, db `rpgworkspace`) / `appsettings.Development.json` (db `rpgworkspace_dev`). Both files currently commit real-looking dev secrets (JWT key, DB password) — treat them as placeholders to replace before any real deployment, not as secrets to protect in this repo.
- `src/RpgWorkspace.Api/RpgWorkspace.Api.http` still references the scaffolded `/weatherforecast/` endpoint from the project template — it doesn't reflect real routes; don't rely on it.

## Architecture

### Layering

```
RpgWorkspace.Api             → controllers, DI wiring entrypoint (Program.cs), auth/swagger/health-check extensions, exception middleware
RpgWorkspace.Application     → DTOs, service interfaces, service implementations (use cases + authorization logic)
RpgWorkspace.Domain          → entities, enums; no dependencies on other layers
RpgWorkspace.Infrastructure  → EF Core DbContext + configurations + migrations, repositories, UnitOfWork, JWT/BCrypt, and two services (Search, Dashboard) that read the DbContext directly
```

Dependency direction: Api → Infrastructure + Application → Domain. Infrastructure implements the interfaces declared in Application (`IXxxRepository`, `IXxxService` for cross-cutting reads, `ITokenGenerator`, `IPasswordHasher`, `IUnitOfWork`). All registrations live in one place: `RpgWorkspace.Infrastructure/DependencyInjection.cs` (`AddInfrastructure`) — when adding a new module, register its repository + service there.

### Request flow and the authorization pattern

Every write/read for workspace-scoped resources follows the same shape, best seen in `CampaignsController` → `CampaignService`:

1. Controller is `[Authorize]`, extracts `Guid userId` from the JWT's `NameIdentifier`/`sub` claim (helper method repeated per controller, not centralized — see `GetCurrentUserId()`).
2. Controller calls the service method, passing `requestingUserId` alongside the DTO.
3. Service loads the owning `Workspace` **with its `Members` included** and checks membership/role via `Workspace.IsMember`, `IsOwnerOrMaster`, `IsOwner` (methods on the `Workspace` aggregate itself — this is where role logic lives, not in the controller or in ASP.NET policies).
4. Service throws `KeyNotFoundException` for "not found or not a member" (deliberately conflated — being a non-member of a workspace looks identical to the resource not existing, so membership isn't leaked) and `UnauthorizedAccessException` for "found, but role insufficient".
5. Controller catches those two exception types per-action and maps them to 404 / 403 respectively. There's no global exception filter for these — `ExceptionMiddleware` only handles the *unhandled* fallthrough case (which it turns into a generic 500, hiding stack traces unless `IsDevelopment()`).

This pattern (load parent → check role via aggregate method → throw specific exception → controller maps to status code) is the convention for **every** new resource. Follow it rather than introducing `[Authorize(Policy = ...)]` or claims-based authorization checks.

Two content-visibility tiers layer on top of workspace roles, per entity:
- **Owner/Master vs Player role** — gates create/update/delete on most sub-resources (Session, Npc, Location, Quest, Character, etc.) and workspace/campaign management itself.
- **Public vs private content** — `IsPrivate` (bool, on Npc/Location/Quest) or `Visibility` (enum `WikiVisibility`/`WorldLibraryVisibility` with a `MastersOnly`/`Public` split, on WikiPage/WorldLibraryItem) additionally hides individual records from Players even though they can see the resource list.

A third, narrower tier exists for `Character`-scoped content (PlayerNote, Theory, Operation, ImportantPerson, NarrativeItem): visible to the workspace's Owner/Master, **or** the specific `User` who owns that `Character` (see `DashboardService.EnsureCanViewCharacterDashboard` for the canonical check) — i.e. one player's character journal is private from other players by default.

### Domain entities

- All entities inherit `BaseEntity` (`Domain/Common/BaseEntity.cs`): `Guid Id`, `CreatedAt`, nullable `UpdatedAt`, protected setters, `SetUpdatedAt()` helper.
- Entities are constructed only through static `Create(...)` factories and mutated through `Update(...)`/behavior methods — constructors are private (EF Core materializes via the private parameterless ctor + backing fields). Keep this style for new entities/DTOs rather than adding public setters.
- Two parallel content trees hang off `Campaign`:
  - **GM/world content**: `Session`, `Npc`, `Location`, `Quest`, `WikiPage`, `WorldLibraryItem` — workspace-role-gated, optionally `IsPrivate`/`Visibility`-gated.
  - **Player/character content**: `Character` (belongs to a `User` within a `Campaign`) owns `PlayerNote`, `ImportantPerson`, `Theory`, `Operation`, `NarrativeItem` — this is effectively a private investigation journal per player character, aggregated by `DashboardService.GetCharacterDashboardAsync`.
- `Workspace` also owns `WorkspaceMember` (role: Owner/Master/Player) and `WorkspaceInvite` (invite flow, since members can't self-join).
- `ScheduleEvent`/`ScheduleResponse` handle session-scheduling polls (proposed dates + per-user RSVP responses), separate from `Session` (the actual played/logged session record).

### Tagging system

`Tag` is scoped to a `Campaign` (not global). Rather than a single polymorphic join table, each taggable entity has its **own** join entity (`SessionTag`, `NpcTag`, `LocationTag`, `QuestTag`, `WikiPageTag`, `WorldLibraryItemTag`, `PlayerNoteTag`, `TheoryTag`, `OperationTag`, `NarrativeItemTag` — all in `Domain/Entities/TagLinks.cs`, all following the identical `Create(entityId, tagId)` shape). When adding tags to a new taggable entity, add a matching `<Entity>Tag` join type here plus a `DbSet` + EF configuration, don't try to generalize into one shared join table.

`TagAssociationHelper` (`Application/Services/TagAssociationHelper.cs`) is the shared validation used by every service that accepts tag IDs on create/update: it rejects tag IDs that don't exist or belong to a different campaign than the entity being tagged (`InvalidOperationException`).

### Cross-cutting read services bypass the repository layer

`SearchService` and `DashboardService` (both in `RpgWorkspace.Infrastructure/Services/`, not `Repositories/`) inject `AppDbContext` directly instead of going through per-entity repositories, because they need ad-hoc cross-entity `AsNoTracking()` LINQ queries (fan-out search across Sessions/Npcs/Locations/Quests/Characters/WikiPages/WorldLibraryItems; dashboard aggregation/counts across many tables). This is an intentional exception to the repository pattern used everywhere else — follow it for future read-only, multi-entity aggregation endpoints rather than forcing them through `IUnitOfWork`/repositories.

### Persistence

- `AppDbContext` (`Infrastructure/Persistence/AppDbContext.cs`) exposes one `DbSet` per entity/join-entity; `OnModelCreating` calls `ApplyConfigurationsFromAssembly` so every `IEntityTypeConfiguration<T>` in `Persistence/Configurations/` is picked up automatically — add a configuration class there for any new entity rather than configuring inline.
- Migrations (`Persistence/Migrations/`) are one-per-module and named accordingly (`InitialCreate`, `AddWorkspaceModule`, `AddCampaignModule`, `AddSessionModule`, ... `AddTagModule`) — keep new modules' migrations similarly scoped/named rather than bundling unrelated schema changes into one migration.
- Provider is Npgsql/PostgreSQL; `EF.Functions.ILike` is used for case-insensitive search (see `SearchService`) — stick to Postgres-specific operators knowingly, this isn't provider-agnostic.

### Auth

JWT Bearer only (`AuthExtensions.AddJwtAuthentication`), configured from the `JwtSettings` section (`Secret`/`Issuer`/`Audience`/`ExpirationMinutes`). No refresh tokens, no cookie auth, no external identity providers. Password hashing is BCrypt (`BcryptPasswordHasher`). There's no `[Authorize(Roles=...)]` usage — all role/permission logic is manual, in the Application-layer services, as described above.
