# VetGest

VetGest is a veterinary pregnancy tracking PWA for Tutors and Vets. It supports organization, education, reminders, a shared diary, and safety prompts. It does not replace veterinary consultation, diagnosis, or treatment.

## Repository structure

```text
VetGest/
├── .github/                  # AI governance: instructions, agents, skills, prompts
├── src/
│   ├── Backend/
│   │   ├── VetGest.Domain/
│   │   ├── VetGest.Application/
│   │   ├── VetGest.Infrastructure/
│   │   └── VetGest.API/
│   ├── Frontend/
│   │   └── VetGest.Web/
│   └── Shared/
│       └── VetGest.Contracts/
├── tests/
│   ├── VetGest.Domain.Tests/
│   └── VetGest.Application.Tests/
├── VetGest.Backend.sln
└── VetGest.Web.sln
```

## Solutions

- `VetGest.Backend.sln` contains Domain, Application, Infrastructure, API, Contracts, and backend tests.
- `VetGest.Web.sln` contains the Blazor WebAssembly PWA and Contracts.
- `VetGest.Contracts` is shared by both solutions and must not depend on persistence or UI frameworks.

## AI-first workflow

1. Read `.github/copilot-instructions.md` and the applicable scoped instruction.
2. Select the relevant custom agent or skill.
3. State the owning boundary, assumptions, and focused validation before editing.
4. Implement one small vertical slice.
5. Run focused tests or builds immediately, then validate both solutions.

AI governance is part of the product foundation. Do not bypass security, data-isolation, testing, or veterinary-content rules for speed.

## Local prerequisites

- .NET SDK 10.0.400 or a compatible .NET 10 SDK selected by `global.json`.
- A current browser supporting WebAssembly and service workers.

## Build

```powershell
dotnet restore VetGest.Backend.sln
dotnet build VetGest.Backend.sln

dotnet restore VetGest.Web.sln
dotnet build VetGest.Web.sln
dotnet test VetGest.Backend.sln
```

## Azure deployment

The GitHub Actions workflow builds and tests both solutions, publishes the API and Web as separate artifacts, and deploys them only after a push to `main`.

The current database configuration is PostgreSQL via Npgsql. The infrastructure expects `ConnectionStrings__DefaultConnection` to be a PostgreSQL connection string, not an Azure SQL connection string.

Configure these GitHub production environment values before enabling deployment:

- Variable `VETGEST_API_APP_NAME`: Azure App Service name for the API.
- Variable `VETGEST_API_BASE_URL`: public HTTPS URL of the API, including the trailing slash.
- Secret `AZURE_CLIENT_ID`: client ID of the Microsoft Entra application used by GitHub Actions.
- Secret `AZURE_TENANT_ID`: Microsoft Entra tenant ID.
- Secret `AZURE_SUBSCRIPTION_ID`: Azure subscription ID.
- Secret `VETGEST_STATIC_WEB_APPS_TOKEN`: deployment token for the Static Web App.

The Entra application must have a federated credential for this GitHub repository and the `main` branch, plus permission to deploy the `vetgest-api` App Service. The API job uses OIDC through `azure/login`; it does not use an App Service publish profile.

Configure these API App Settings in Azure App Service, without committing them:

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string for the API database.
- `Jwt__SecretKey`: production JWT signing key.
- `Jwt__Issuer` and `Jwt__Audience`: production token values.
- `Cors__AllowedOrigins__0`: public HTTPS URL of the Static Web App.
- `Admin__Email` and `Admin__Password`: bootstrap administrator, created at startup if no `Admin` exists (locally use `dotnet user-secrets`). Remove `Admin__Password` after the first start.

### Roles, vet connection and chat

- Logged out: only Entrar / Criar conta. After login each role lands on its own area: Admin → `/admin/vets`, Vet → `/vet-connection`, Tutor → `/pets`.
- A vet generates a one-time code (`POST api/vet-connections/codes`). The tutor enters it at `/vet-connection` and chooses which active pregnancy to share (`POST api/vet-connections/accept` with `PetId` and `PregnancyId`). Either party can end the connection; that cuts diary access and chat immediately. The older `POST api/vet-connections/invitations` endpoint still works but is deprecated and no longer used by the UI.
- A linked vet sees the shared pet, an estimated due date and the tutor's diary entries **read-only** (`/vet/patients`, `GET api/vet/patients`). Access requires an Active connection owned by that vet.
- Live chat (`/chat`, SignalR hub `/hubs/chat`) works between the two participants of an Active connection. Messages are relayed only and are **not stored**; membership is re-checked on every join and send, and the UI shows that the chat is not for emergencies.
- SignalR clients authenticate with `access_token` in the query string (accepted only for `/hubs/*`).
### Accounts and CRMV

Public sign-up only creates `Tutor` accounts. Veterinarians are created by an administrator at `/admin/vets`, which returns a one-time link (`/definir-senha`) for the vet to choose a password. The CRMV (number + UF) is validated only for format; there is no CFMV API (the search at `app.cfmv.gov.br` is reCAPTCHA-protected), so the administrator must confirm name, number, UF and active status there before creating the account.

### Supabase PostgreSQL

Use the Supavisor **session pooler** string (Project Settings > Database > Connection string), because the direct host is IPv6-only and many hosts cannot reach it. The username is `postgres.<project-ref>`. Keep the Npgsql pool small and recycle idle connections so pooled sockets dropped by Supabase or the host network are not reused:

```
Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=10;Keepalive=30;Connection Idle Lifetime=30;Timeout=15;Command Timeout=30
```

If you use the transaction pooler (port 6543), also add `No Reset On Close=true` and do not enable `Max Auto Prepare`. Free projects pause after inactivity and must be resumed in the Supabase dashboard.

The free or consumption tiers may sleep, have quotas, and are not equivalent to production-grade availability. Apply database migrations through a controlled release step before serving traffic; production startup does not apply migrations automatically unless `Database__MigrateOnStartup=true` is set. Roles are seeded in the background after the API starts listening, so port 8080 opens without waiting for the database.

## Authenticated pet registration backend

The backend now exposes authenticated vertical slices for account, pet, pregnancy, diary, alerts, and Vet-Tutor connection:

- `POST /api/auth/register` creates an active Tutor account and returns `ApiResponse<AuthResponse>` with a JWT. Any requested Vet role is ignored; Vet accounts must be provisioned separately.
- `POST /api/auth/login` validates the account and password and returns the same envelope. Identity failures use generic messages.
- `POST /api/pets` creates a Dog or Cat for the authenticated Tutor and returns `ApiResponse<PetDto>`.
- `GET /api/pets` lists only the authenticated user's pets in `ApiResponse<IReadOnlyList<PetDto>>`.
- `GET /api/pets/{id}` returns the authenticated user's pet or `404`; an unrelated ID also returns `404` to avoid resource enumeration.
- `POST /api/vet-connections/invitations` requires role `Vet` and creates an invitation for a tutor-owned pregnancy.
- `POST /api/vet-connections/accept` requires role `Tutor` and accepts a valid invitation targeted to the authenticated tutor.
- `POST /api/vet-connections/{connectionId}/revoke` allows only linked participants (Tutor or Vet) to revoke a connection.
- `GET /api/vet-connections` lists connections visible to the authenticated user.
- `GET/POST /hubs/vet-connections` provides authenticated SignalR collaboration with per-pregnancy join authorization.

Use cases live in `VetGest.Application`; EF persistence sets and filters the `OwnerId` shadow property in Infrastructure. The existing EF baseline `20260904191139_InitialCreate` remains unchanged. The repository now includes an API integration-test host focused on authorization and linked-record isolation for vet-connection endpoints and hub access, while broader auth-flow coverage (for example registration/login identity edge cases and full SQL-container integration) remains a follow-up gap.
