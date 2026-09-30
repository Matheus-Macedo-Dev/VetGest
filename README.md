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

The current database remains Azure SQL. No Supabase or PostgreSQL migration is part of this deployment slice.

Configure these GitHub production environment values before enabling deployment:

- Variable `VETGEST_API_APP_NAME`: Azure App Service name for the API.
- Variable `VETGEST_API_BASE_URL`: public HTTPS URL of the API, including the trailing slash.
- Secret `AZURE_CLIENT_ID`: client ID of the Microsoft Entra application used by GitHub Actions.
- Secret `AZURE_TENANT_ID`: Microsoft Entra tenant ID.
- Secret `AZURE_SUBSCRIPTION_ID`: Azure subscription ID.
- Secret `VETGEST_STATIC_WEB_APPS_TOKEN`: deployment token for the Static Web App.

The Entra application must have a federated credential for this GitHub repository and the `main` branch, plus permission to deploy the `vetgest-api` App Service. The API job uses OIDC through `azure/login`; it does not use an App Service publish profile.

Configure these API App Settings in Azure App Service, without committing them:

- `ConnectionStrings__DefaultConnection`: Azure SQL connection string.
- `Jwt__SecretKey`: production JWT signing key.
- `Jwt__Issuer` and `Jwt__Audience`: production token values.
- `Cors__AllowedOrigins__0`: public HTTPS URL of the Static Web App.

The free or consumption tiers may sleep, have quotas, and are not equivalent to production-grade availability. Apply database migrations through a controlled release step before serving traffic; production startup does not apply migrations automatically.

## Authenticated pet registration backend

The backend now exposes the first authenticated vertical slice:

- `POST /api/auth/register` creates an active Tutor account and returns `ApiResponse<AuthResponse>` with a JWT. Any requested Vet role is ignored; Vet accounts must be provisioned separately.
- `POST /api/auth/login` validates the account and password and returns the same envelope. Identity failures use generic messages.
- `POST /api/pets` creates a Dog or Cat for the authenticated Tutor and returns `ApiResponse<PetDto>`.
- `GET /api/pets` lists only the authenticated user's pets in `ApiResponse<IReadOnlyList<PetDto>>`.
- `GET /api/pets/{id}` returns the authenticated user's pet or `404`; an unrelated ID also returns `404` to avoid resource enumeration.

Pet use cases live in `VetGest.Application`; EF persistence sets and filters the `OwnerId` shadow property in Infrastructure. The existing EF baseline `20260904191139_InitialCreate` remains unchanged. The repository currently has unit coverage for pet validation and ownership, but no API integration-test host, SQL test container, or Identity test fixture; endpoint-level registration/login and anonymous HTTP assertions remain an integration coverage gap.
