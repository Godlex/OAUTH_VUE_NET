# OAUTH_VUE_NET — Inventory App

Full-stack **inventory management** app with OAuth 2.0 (Duende IdentityServer), .NET 10 WebAPI, Vue 3, and PostgreSQL. Demonstrates Clean Architecture, CQRS, and Authorization Code + PKCE.

---

## Table of Contents

- [What It Does](#what-it-does)
- [Tech Stack](#tech-stack)
- [Quick Start](#quick-start)
- [Running the App](#running-the-app)
- [Test Accounts](#test-accounts)
- [URLs & Ports](#urls--ports)
- [Project Structure](#project-structure)
- [Auth Flow (OAuth 2.0 + PKCE)](#auth-flow-oauth-20--pkce)
- [API Reference](#api-reference)
- [Troubleshooting](#troubleshooting)

---

## What It Does

- **Product inventory**: list, add, edit, delete products (name, category, price, quantity).
- **Login** is handled by a separate **Identity Server**; the Vue app and WebAPI only use JWT tokens.
- **Test users** (`alice`, `bob`) are seeded on first run; login accepts **username or email**.

---

## Tech Stack

| Layer        | Technology                          |
|-------------|--------------------------------------|
| Auth        | Duende IdentityServer (.NET 10)     |
| API         | ASP.NET Core Web API (.NET 10)       |
| Business    | MediatR, FluentValidation (CQRS)     |
| Database    | PostgreSQL 16 (EF Core)               |
| Frontend    | Vue 3, TypeScript, Vite, Pinia       |
| Auth client | oidc-client-ts (Authorization Code + PKCE) |

**Two databases:** `identity_db` (users) and `inventory_db` (products). IdentityServer and WebAPI each use one; they do not share DBs.

---

## Quick Start

### Option A — Docker (all services)

Ensure [Docker Desktop](https://www.docker.com/products/docker-desktop/) is **running**, then:

```bash
docker compose up -d --build
```

Open **http://localhost:5173**, sign in with **alice** / **Alice123!**.

Logs: `docker compose logs -f`. Stop: `docker compose down`.

### Option B — PowerShell launcher (Windows)

```powershell
.\start.ps1
```

Starts PostgreSQL (via Docker if available), IdentityServer, WebAPI, and Vue in separate windows and opens the app. Optional: `-SkipDocker` if PostgreSQL is already running; `-NoBrowser` to skip opening the browser.

### Option C — Manual (3 terminals + Postgres)

1. **PostgreSQL:** `docker compose up -d` (or use existing Postgres; set connection strings in `appsettings.json`).
2. **IdentityServer:** `cd backend\src\IdentityServer\OAUTH_VUE_NET.IdentityServer && dotnet run`
3. **WebAPI:** `cd backend\src\WebAPI\OAUTH_VUE_NET.WebAPI && dotnet run`
4. **Frontend:** `cd frontend && npm run dev`

Then open http://localhost:5173.

---

## Running the App

### Prerequisites

- **.NET SDK 10** — [download](https://dotnet.microsoft.com/download)
- **Node.js 20+** — [download](https://nodejs.org/)
- **Docker Desktop** — for PostgreSQL (and full Docker run). Start the app before `docker compose` commands.

Check:

```bash
dotnet --version   # 10.x
node --version     # v20+
```

Trust the dev HTTPS certificate (for IdentityServer and WebAPI):

```bash
dotnet dev-certs https --trust
```

### Docker (Option A details)

- **Build and run:** `docker compose up -d --build`
- **Logs:** `docker compose logs -f` or `docker compose logs -f identityserver`, etc.
- **Stop:** `docker compose down`

When using Docker, the frontend is built with `VITE_AUTHORITY=http://localhost:5000` and `VITE_API_URL=http://localhost:5002` so the browser can reach auth and API on the host.

### Local (Option B/C details)

- **IdentityServer** listens on **https://localhost:5001** and **http://localhost:5000**.
- **WebAPI** on **https://localhost:5002** and **http://localhost:5003**.
- **Vue (Vite)** on **http://localhost:5173**.

IdentityServer and WebAPI run migrations and seed data on startup. No manual migration steps required.

---

## Test Accounts

Seeded by IdentityServer on first run. You can use **username or email** to sign in.

| Username | Password   | Email             |
|----------|------------|-------------------|
| alice    | Alice123!  | alice@example.com |
| bob      | Bob123!    | bob@example.com   |

If an account is locked after failed attempts, restart IdentityServer; seed logic unlocks these test users on startup.

---

## URLs & Ports

| When running   | Frontend      | WebAPI / Swagger     | IdentityServer      | pgAdmin   |
|----------------|---------------|----------------------|---------------------|-----------|
| **Docker**     | :5173         | :5002/swagger        | :5000               | :5050     |
| **Local**      | :5173         | :5002/swagger (HTTPS)| :5001 (HTTPS), :5000| :5050     |

- **Docker:** Frontend at http://localhost:5173, API at http://localhost:5002, Identity at http://localhost:5000.
- **Local:** Prefer HTTPS for IdentityServer and WebAPI (e.g. https://localhost:5001, https://localhost:5002).

---

## Project Structure

```
OAUTH_VUE_NET/
├── backend/
│   ├── src/
│   │   ├── Data/           # EF Core, Product entity, repositories
│   │   ├── BLL/            # CQRS: commands, queries, handlers, MediatR
│   │   ├── WebAPI/         # REST API, JWT validation, Swagger
│   │   └── IdentityServer/ # Duende IS, login UI, user DB, SeedData
│   └── OAUTH_VUE_NET.slnx
├── frontend/               # Vue 3 + Vite, Pinia, Vue Router
│   ├── src/
│   │   ├── api/            # productsApi (Axios + JWT)
│   │   ├── services/       # authService (oidc-client-ts)
│   │   ├── stores/         # auth store (Pinia)
│   │   ├── views/          # Home, Products, Callback, SilentRenew
│   │   └── router/
│   ├── Dockerfile
│   └── nginx.conf          # for Docker serve
├── docker-compose.yml      # postgres, pgadmin, identityserver, webapi, frontend
├── start.ps1               # Windows: start all services
└── stop.ps1                # Stop local processes (if used)
```

- **WebAPI** depends on **BLL** and **Data**; **IdentityServer** is standalone with its own DB.
- **Frontend** uses `VITE_AUTHORITY` and `VITE_API_URL` when built for Docker (see Dockerfile build args).

---

## Auth Flow (OAuth 2.0 + PKCE)

1. User clicks **Sign In** in Vue → redirect to IdentityServer `/connect/authorize` with `code_challenge` (PKCE).
2. User logs in on IdentityServer (username or email + password).
3. IdentityServer redirects back to Vue `/callback?code=...`.
4. Vue exchanges `code` + `code_verifier` at IdentityServer `/connect/token` → receives access + refresh tokens.
5. Vue stores tokens and sends `Authorization: Bearer <access_token>` to the WebAPI.
6. WebAPI validates the JWT with IdentityServer (e.g. discovery document and JWKs).

PKCE is used because the client is a public SPA (no client secret). Test client: `vue-client`; redirect URIs include `/callback` and `/silent-renew`.

---

## API Reference

**Base URL (local):** `https://localhost:5002/api`  
**Base URL (Docker):** `http://localhost:5002/api`

All endpoints require: `Authorization: Bearer <access_token>`.

| Method   | Endpoint           | Description      |
|----------|--------------------|------------------|
| GET      | /products          | List all products|
| GET      | /products/{id}     | Get one product  |
| POST     | /products          | Create product   |
| PUT      | /products/{id}     | Update product   |
| DELETE   | /products/{id}     | Delete product   |

Request/response use JSON; create/update body matches the product fields (name, description, price, quantity, category). See Swagger at `/swagger` when the WebAPI is running.

---

## Troubleshooting

### Docker: "failed to connect to the docker API" / "pipe dockerDesktopLinuxEngine"

Docker daemon is not running. **Start Docker Desktop** and wait until it is fully up, then run `docker compose` again.

### "Invalid username or password"

- Use **alice** / **Alice123!** or **bob** / **Bob123!** (or alice@example.com / Alice123!).
- Type the password; avoid pasting characters that might be different (e.g. smart quotes).
- If the account is locked, restart IdentityServer to unlock test users.

### Products page: network error or 401

- Ensure **IdentityServer** is running first, then WebAPI, then log in again.
- If using HTTPS locally, run `dotnet dev-certs https --trust` and reload.

### "Connection refused" to database

- Start PostgreSQL: `docker compose up -d` (or use your existing Postgres and correct connection strings in both `appsettings.json` files).

### Port already in use

- Change ports in the relevant `Properties/launchSettings.json` or stop the process using the port.

### Vue blank after login

- Check the browser console (F12). Often caused by untrusted HTTPS cert for IdentityServer/WebAPI — run `dotnet dev-certs https --trust`.

---

## License & Duende

Duende IdentityServer shows a license warning in development; production use requires a [Duende license](https://duende.link/l/contact).
