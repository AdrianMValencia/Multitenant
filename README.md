# Velora — documentación técnica

Plataforma de docs (VitePress) del ERP/CRM **multitenant**.

## Aplicación

- API: `https://localhost:7299`
- Web: `https://localhost:7179/login`
- PostgreSQL: `localhost:5433` / `multitenant_db`

## Documentación interactiva

```bash
cd docs-platform
npm install
npm run docs:dev
```

Índice: arquitectura C4, entorno, BD, backend archivo a archivo, API, Blazor, seguridad, DevOps, troubleshooting, modo “aprender desde cero”.

## Arranque rápido de la app

```bash
dotnet restore
dotnet ef database update --project src/Multitenant.Infrastructure --startup-project src/Multitenant.Api
dotnet run --project src/Multitenant.Api --launch-profile https
dotnet run --project src/Multitenant.Web/Multitenant.Web --launch-profile https
```

Demo: Acme `admin@acme.local` / `Admin123!` / tenant `10000000-0000-0000-0000-000000000001`.
