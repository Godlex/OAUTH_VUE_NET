# InventoryApp — .NET 10 + Vue.js 3 + Duende IdentityServer

A full-stack **Inventory Management** application that demonstrates modern enterprise patterns:
OAuth 2.0 authentication, Clean Architecture, and CQRS — explained from first principles.

> **You will understand:** what each project does, why it exists, how the pieces talk to each other, and how to run the whole thing yourself.

---

## Table of Contents

1. [What This App Does](#1-what-this-app-does)
2. [Technology Stack](#2-technology-stack)
3. [Big Picture: How It All Fits Together](#3-big-picture-how-it-all-fits-together)
4. [Directory & File Structure](#4-directory--file-structure)
5. [Backend Architecture — Clean Layers](#5-backend-architecture--clean-layers)
   - [Data Layer](#51-data-layer--oauth_vue_netdata)
   - [BLL — Business Logic & CQRS](#52-bll--business-logic-layer--oauth_vue_netbll)
   - [WebAPI](#53-webapi--oauth_vue_netwebapi)
   - [IdentityServer](#54-identityserver--oauth_vue_netidentityserver)
6. [Frontend — Vue.js 3](#6-frontend--vuejs-3)
7. [Authentication Flow — Step by Step](#7-authentication-flow--step-by-step)
8. [CQRS — What It Is and How It Works Here](#8-cqrs--what-it-is-and-how-it-works-here)
9. [Database — Entity Framework Core + SQLite](#9-database--entity-framework-core--sqlite)
10. [Prerequisites](#10-prerequisites)
11. [Running the Application](#11-running-the-application)
12. [Test Credentials](#12-test-credentials)
13. [API Reference](#13-api-reference)
14. [Common Problems & Fixes](#14-common-problems--fixes)
15. [Glossary](#15-glossary)

---

## 1. What This App Does

This is a **product inventory manager**. Users can:

- View a list of products (name, category, price, quantity)
- Add new products
- Edit existing products
- Delete products

But **only authenticated users** can do any of that. Before you can see products, you must log in. The login does not happen inside the main app — it is handled by a dedicated **Identity Server** that issues secure tokens (JWTs) proving who you are.

This is the same pattern used by "Sign in with Google" or "Sign in with GitHub" — just self-hosted.

---

## 2. Technology Stack

| Layer | Technology | What it does |
|---|---|---|
| Auth Server | **Duende IdentityServer** (.NET 10) | Issues JWT tokens, hosts the login page |
| API | **ASP.NET Core Web API** (.NET 10) | Serves product data, validates JWT tokens |
| Business Logic | **MediatR** + **FluentValidation** | CQRS handlers and input validation |
| Database | **Entity Framework Core** + **PostgreSQL** | Stores products and user accounts |
| Frontend | **Vue.js 3** + TypeScript + Vite | Single-page app (SPA) the user sees in the browser |
| Auth Client | **oidc-client-ts** | Handles the OAuth login flow inside Vue |
| State | **Pinia** | Vue global state store (who is logged in, token) |
| Routing | **Vue Router** | Client-side navigation, route guards |

---

## 3. Big Picture: How It All Fits Together

There are **3 separate running servers** and they communicate like this:

```
┌──────────────────────────────────────────────────────────────────────┐
│                         USER'S BROWSER                               │
│                                                                      │
│   Vue.js 3 SPA  ──── http://localhost:5173                           │
│       │                                                              │
│       │  1. "I need to log in" → redirects to IdentityServer        │
│       │  4. Gets back an Authorization Code                          │
│       │  5. Exchanges code for JWT tokens (access + refresh)         │
│       │  6. Sends JWT in Authorization header to WebAPI              │
└───────┼──────────────────────────────────────────────────────────────┘
        │
        │ (2) Login form             (6) GET /api/products
        ▼                             + Bearer <JWT>
┌───────────────────┐        ┌──────────────────────────┐
│  IdentityServer   │        │         WebAPI           │
│  :5001            │        │         :5002            │
│                   │        │                          │
│  - Login UI       │        │  - ProductsController    │
│  - Issues JWTs    │        │  - Validates JWT against │
│  - User database  │◄───────│    IdentityServer        │
│                   │  (3)   │  - Calls BLL via MediatR │
└───────────────────┘  Token │  - Returns JSON          │
                       Validation
                             └──────────────────────────┘
                                          │
                                          ▼
                             ┌──────────────────────────┐
                             │   SQLite Database        │
                             │   inventory.db           │
                             │   - Products table       │
                             └──────────────────────────┘
```

**Key rule:** The Vue app never talks to the Identity Server's user database directly. It only gets a token (a JWT — a signed string), and sends that token with every API request to prove its identity.

---

## 4. Directory & File Structure

```
OAUTH_VUE_NET/
│
├── README.md                          ← you are here
│
├── backend/
│   ├── OAUTH_VUE_NET.sln              ← Visual Studio solution (groups all .NET projects)
│   └── src/
│       │
│       ├── Data/
│       │   └── OAUTH_VUE_NET.Data/
│       │       ├── ApplicationDbContext.cs        ← EF Core entry point to the DB
│       │       ├── Entities/
│       │       │   └── Product.cs                 ← What a "Product" looks like in the DB
│       │       ├── Repositories/
│       │       │   ├── IProductRepository.cs      ← Contract: what operations exist
│       │       │   └── ProductRepository.cs       ← Implementation: SQL via EF Core
│       │       └── Extensions/
│       │           └── ServiceCollectionExtensions.cs  ← Registers Data services in DI
│       │
│       ├── BLL/
│       │   └── OAUTH_VUE_NET.BLL/
│       │       ├── Commands/Products/
│       │       │   ├── CreateProductCommand.cs    ← "I want to create a product"
│       │       │   ├── UpdateProductCommand.cs    ← "I want to update a product"
│       │       │   └── DeleteProductCommand.cs    ← "I want to delete a product"
│       │       ├── Queries/Products/
│       │       │   ├── GetProductsQuery.cs        ← "Give me all products"
│       │       │   └── GetProductByIdQuery.cs     ← "Give me product #5"
│       │       ├── Handlers/Products/
│       │       │   ├── CreateProductHandler.cs    ← Does the actual create work
│       │       │   ├── UpdateProductHandler.cs    ← Does the actual update work
│       │       │   ├── DeleteProductHandler.cs    ← Does the actual delete work
│       │       │   ├── GetProductsHandler.cs      ← Does the actual list work
│       │       │   └── GetProductByIdHandler.cs   ← Does the actual fetch work
│       │       ├── Behaviors/
│       │       │   └── ValidationBehavior.cs      ← Runs FluentValidation before every handler
│       │       ├── DTOs/
│       │       │   └── ProductDto.cs              ← Safe data shape returned to the API
│       │       └── Extensions/
│       │           └── ServiceCollectionExtensions.cs  ← Registers BLL (MediatR) in DI
│       │
│       ├── WebAPI/
│       │   └── OAUTH_VUE_NET.WebAPI/
│       │       ├── Controllers/
│       │       │   └── ProductsController.cs      ← HTTP endpoints: GET, POST, PUT, DELETE
│       │       ├── Program.cs                     ← App startup, middleware, JWT config
│       │       ├── appsettings.json               ← Configuration (DB connection, IS URL)
│       │       └── Properties/
│       │           └── launchSettings.json        ← Port 5002 for dev
│       │
│       └── IdentityServer/
│           └── OAUTH_VUE_NET.IdentityServer/
│               ├── Config.cs                      ← Which clients, scopes, resources exist
│               ├── SeedData.cs                    ← Creates test users (alice, bob) on startup
│               ├── Program.cs                     ← App startup, Identity, IdentityServer config
│               ├── appsettings.json               ← DB connection string
│               ├── Data/
│               │   └── IdentityDbContext.cs       ← User accounts database
│               ├── Pages/                         ← Razor Pages (server-rendered HTML)
│               │   ├── Account/Login/             ← The login form (GET shows it, POST submits)
│               │   ├── Account/Logout/            ← The logout page
│               │   └── Error/                     ← Error display page
│               └── wwwroot/css/
│                   └── site.css                   ← Styles for the login page
│
└── frontend/
    ├── index.html                     ← HTML shell, Vue mounts here
    ├── vite.config.ts                 ← Vite build tool config
    ├── package.json                   ← npm dependencies
    └── src/
        ├── main.ts                    ← App entry point: creates Vue app, boots auth
        ├── App.vue                    ← Root component: NavBar + <RouterView />
        ├── router/
        │   └── index.ts               ← Routes + navigation guard (blocks unauthenticated)
        ├── stores/
        │   └── auth.ts                ← Pinia store: isAuthenticated, userName, token
        ├── services/
        │   └── authService.ts         ← Wraps oidc-client-ts: login, logout, callback
        ├── api/
        │   └── productsApi.ts         ← Axios calls to the WebAPI (auto-adds JWT)
        ├── components/
        │   └── NavBar.vue             ← Top navigation: links, user greeting, sign out
        └── views/
            ├── HomeView.vue           ← Public landing page
            ├── ProductsView.vue       ← Protected products table + CRUD modal
            ├── CallbackView.vue       ← Handles OAuth redirect after login
            └── SilentRenewView.vue    ← Silent token refresh (background iframe)
```

---

## 5. Backend Architecture — Clean Layers

The backend is split into **4 separate .NET projects**. Each project has one responsibility and knows nothing about the layers above it.

```
WebAPI  ──depends on──►  BLL  ──depends on──►  Data
                                                  │
IdentityServer (standalone, owns its own DB)      ▼
                                               SQLite
```

The golden rule: **Data does not know about BLL. BLL does not know about WebAPI.** This makes it easy to change, test, or replace any one layer without touching the others.

---

### 5.1 Data Layer — `OAUTH_VUE_NET.Data`

**Responsibility:** Talk to the database. Nothing else.

#### `Entities/Product.cs`

This is the C# class that maps 1-to-1 to a row in the `Products` database table.

```csharp
public class Product
{
    public int Id { get; set; }         // auto-incremented primary key
    public string Name { get; set; }    // "Laptop"
    public decimal Price { get; set; }  // 1299.99
    public int Quantity { get; set; }   // 15
    public string Category { get; set; }// "Electronics"
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } // null if never updated
}
```

#### `ApplicationDbContext.cs`

This is the EF Core "session" with the database. Any time you want to read or write products, you go through this class. It also holds **seed data** — 3 sample products that are inserted automatically when the database is first created.

#### `Repositories/IProductRepository.cs` and `ProductRepository.cs`

Instead of writing raw SQL or calling EF Core directly from the business logic, we hide all database operations behind an **interface** (a contract):

```csharp
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(...);
    Task<Product?> GetByIdAsync(int id, ...);
    Task<Product> CreateAsync(Product product, ...);
    Task<Product> UpdateAsync(Product product, ...);
    Task<bool> DeleteAsync(int id, ...);
}
```

`ProductRepository` is the concrete class that implements this interface using EF Core.

**Why the interface?** Because the BLL only talks to `IProductRepository`. In tests you can swap in a fake implementation without touching a real database.

---

### 5.2 BLL — Business Logic Layer — `OAUTH_VUE_NET.BLL`

**Responsibility:** Contain all business rules. Decide *what* happens, not *how* it is stored or *how* it is exposed over HTTP.

This layer implements **CQRS** (see [Section 8](#8-cqrs--what-it-is-and-how-it-works-here) for a full explanation).

#### `DTOs/ProductDto.cs`

DTO = **Data Transfer Object**. Instead of sending the raw `Product` entity (which is tied to the database) out of the API, we convert it to a clean, simple record:

```csharp
public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Quantity,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
```

Records in C# are immutable — once created, their values cannot change. This prevents accidental modification.

#### Commands (write operations)

A **Command** is a plain C# record that describes an *intention* — something the user wants to change. It carries all the data needed for that change.

```csharp
// "Please create a product with these values"
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Quantity,
    string Category
) : IRequest<ProductDto>;   // ← IRequest means MediatR will route this to a handler
```

There are 3 commands: `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`.

#### Queries (read operations)

A **Query** is a plain record that describes *what data you want*. It never changes anything.

```csharp
// "Give me all products"
public record GetProductsQuery : IRequest<IEnumerable<ProductDto>>;

// "Give me the product with this ID"
public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
```

#### Handlers

Each Command/Query has exactly **one Handler** — a class that knows how to execute it.

```csharp
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository) // injected by DI
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // 1. Build entity from the command data
        var product = new Product { Name = request.Name, Price = request.Price, ... };

        // 2. Save to DB via repository
        var created = await _repository.CreateAsync(product, ct);

        // 3. Convert to DTO and return
        return new ProductDto(created.Id, created.Name, ...);
    }
}
```

#### `Behaviors/ValidationBehavior.cs`

This is a **pipeline behavior** — code that runs automatically *before* every handler. It checks whether the incoming command/query is valid using FluentValidation rules. If validation fails, it throws a `ValidationException` before the handler even runs, saving you from writing `if (name == null)` checks everywhere.

---

### 5.3 WebAPI — `OAUTH_VUE_NET.WebAPI`

**Responsibility:** Expose business logic over HTTP. Protect endpoints with JWT authentication.

#### `Controllers/ProductsController.cs`

This is the HTTP entry point. Each method maps to one HTTP verb + URL:

| Method | URL | What it does | Command/Query used |
|---|---|---|---|
| GET | `/api/products` | List all products | `GetProductsQuery` |
| GET | `/api/products/{id}` | Get one product | `GetProductByIdQuery` |
| POST | `/api/products` | Create a product | `CreateProductCommand` |
| PUT | `/api/products/{id}` | Update a product | `UpdateProductCommand` |
| DELETE | `/api/products/{id}` | Delete a product | `DeleteProductCommand` |

All endpoints have `[Authorize]` — if there is no valid JWT in the request header, the API returns `401 Unauthorized` immediately without even running the handler.

The controller is intentionally thin. It never contains business logic. Its only job is:
1. Receive the HTTP request
2. Build a command/query from the request data
3. Send it to MediatR: `await _mediator.Send(command)`
4. Return the result as HTTP JSON

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
{
    var product = await _mediator.Send(command, ct);  // ← entire business logic is here
    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
}
```

#### `Program.cs`

This is where the application is assembled. Key things configured here:

- **JWT Bearer Authentication** — tells ASP.NET Core to validate Bearer tokens by contacting IdentityServer's discovery document
- **CORS** — allows the Vue frontend at `localhost:5173` to call this API
- **Swagger UI** — auto-generated API documentation at `/swagger`
- **Database auto-creation** — runs `EnsureCreated()` on startup so the SQLite file is created with seed data automatically

---

### 5.4 IdentityServer — `OAUTH_VUE_NET.IdentityServer`

**Responsibility:** Be the single source of truth for user identity. Issue signed JWT tokens. Host the login page.

This project is a **completely standalone web application** — it has its own database, its own Razor Pages UI, and its own HTTP port (5001). The WebAPI and Vue app never touch the user password. They only work with the token that IdentityServer gives them after login.

#### `Config.cs` — The Heart of IdentityServer

This file defines the rules:

```csharp
// What identity information can be requested?
public static IEnumerable<IdentityResource> IdentityResources => [
    new IdentityResources.OpenId(),   // sub (user ID)
    new IdentityResources.Profile(),  // name, username
    new IdentityResources.Email()     // email
];

// What API scopes exist?
public static IEnumerable<ApiScope> ApiScopes => [
    new ApiScope("api1", "Inventory API")  // WebAPI requires this scope
];

// What clients (apps) are allowed to get tokens?
public static IEnumerable<Client> Clients => [
    new Client {
        ClientId = "vue-client",
        AllowedGrantTypes = GrantTypes.Code,    // Authorization Code flow
        RequireClientSecret = false,             // SPA has no secret — it uses PKCE instead
        RequirePkce = true,                      // PKCE protects the auth code exchange
        RedirectUris = { "http://localhost:5173/callback" },
        AllowedScopes = { "openid", "profile", "email", "api1" }
    }
];
```

#### `SeedData.cs`

Runs once when the app starts and creates test users if they do not already exist:

- `alice` / `Alice123!`
- `bob` / `Bob123!`

#### Razor Pages Login UI

`Pages/Account/Login/Index.cshtml` — the HTML login form  
`Pages/Account/Login/Index.cshtml.cs` — the C# code-behind that:
1. Gets the `returnUrl` from the query string (where to redirect after login)
2. Calls `SignInManager.PasswordSignInAsync()` to validate credentials
3. On success: redirects to `returnUrl` (back to Vue's `/callback` route)
4. On failure: shows "Invalid username or password"

---

## 6. Frontend — Vue.js 3

**Responsibility:** The user interface. A Single-Page Application (SPA) that runs entirely in the browser.

### Key Files Explained

#### `src/main.ts` — App Entry Point

```typescript
const authStore = useAuthStore()
authStore.init()      // ← checks localStorage for existing token before showing the app
  .then(() => {
    app.mount('#app') // ← mounts Vue into index.html only after auth state is known
  })
```

This prevents a flash where the user appears logged-out for a split second if they already have a valid token.

#### `src/services/authService.ts` — The Auth Bridge

This wraps the `oidc-client-ts` library. You never call `oidc-client-ts` directly in components — you always go through this service. It exposes simple methods:

```typescript
authService.login()          // starts the OAuth redirect flow
authService.handleCallback() // processes the redirect after login
authService.logout()         // logs out and redirects to IdentityServer logout
authService.getAccessToken() // returns the current JWT, or null
```

#### `src/stores/auth.ts` — Pinia Auth Store

Pinia is Vue's state management library. This store holds the global auth state that any component can read:

```typescript
const authStore = useAuthStore()

authStore.isAuthenticated  // true/false — is there a valid, non-expired token?
authStore.userName         // "alice" — display name from the token
authStore.accessToken      // the raw JWT string to send to the API
```

Any component can call `authStore.login()` or `authStore.logout()`.

#### `src/api/productsApi.ts` — API Client

Uses Axios to call the WebAPI. The key feature is an **Axios interceptor** — code that automatically runs before every request:

```typescript
apiClient.interceptors.request.use(async (config) => {
  const authStore = useAuthStore()
  if (authStore.accessToken) {
    config.headers.Authorization = `Bearer ${authStore.accessToken}` // ← added to every request
  }
  return config
})
```

This means you never have to remember to add the token manually. Every API call automatically has it.

#### `src/router/index.ts` — Navigation Guard

Routes with `meta: { requiresAuth: true }` are protected:

```typescript
router.beforeEach(async (to) => {
  if (to.meta.requiresAuth) {
    const authStore = useAuthStore()
    if (!authStore.isAuthenticated) {
      await authStore.login()  // ← redirects to IdentityServer if not logged in
      return false             // ← stops navigation until login completes
    }
  }
})
```

#### `src/views/CallbackView.vue` — The OAuth Landing Page

After login, IdentityServer redirects the browser to `http://localhost:5173/callback?code=...`. This component:
1. Calls `authStore.handleCallback()` — which exchanges the `code` for real tokens
2. Redirects to `/products`
3. Shows an error message if something went wrong

---

## 7. Authentication Flow — Step by Step

This is the **Authorization Code flow with PKCE**, the industry standard for browser-based apps.

```
STEP 1: User clicks "Sign In" in Vue app
         Vue calls: authService.login()
         oidc-client-ts generates a random PKCE code_verifier (stored in sessionStorage)
         and hashes it to produce code_challenge

STEP 2: Browser redirects to IdentityServer
         https://localhost:5001/connect/authorize
           ?client_id=vue-client
           &response_type=code
           &scope=openid profile api1
           &redirect_uri=http://localhost:5173/callback
           &code_challenge=abc123...   ← the hash (safe to send, not the secret)
           &code_challenge_method=S256

STEP 3: User sees the login page, enters alice / Alice123!
         IdentityServer validates credentials against its database
         Issues a one-time Authorization Code (a short-lived string)

STEP 4: IdentityServer redirects back to Vue
         http://localhost:5173/callback?code=xyz789&state=...

STEP 5: CallbackView.vue runs
         oidc-client-ts sends a POST to IdentityServer's /connect/token endpoint:
           code=xyz789
           code_verifier=<the original secret from step 1>  ← proves this is the same browser
         IdentityServer verifies hash(code_verifier) == code_challenge
         If it matches: issues Access Token (JWT) + Refresh Token

STEP 6: Tokens stored in localStorage
         Vue app is now authenticated
         User is redirected to /products

STEP 7: Every API request to WebAPI
         Authorization: Bearer <access_token JWT>

STEP 8: WebAPI validates the JWT
         Reads the JWKs (public keys) from https://localhost:5001/.well-known/openid-configuration
         Verifies the JWT signature — if valid, the request proceeds
         If invalid/missing: returns 401 Unauthorized

STEP 9: Access token expires after 1 hour
         oidc-client-ts automatically uses the Refresh Token to get a new Access Token
         User never needs to log in again until the Refresh Token also expires
```

**Why PKCE?** In a browser, you cannot safely store a "client secret" (anyone can open DevTools and see it). PKCE replaces the secret with a one-time mathematical proof — only the browser that started the login can complete it.

---

## 8. CQRS — What It Is and How It Works Here

**CQRS = Command Query Responsibility Segregation**

The idea is simple: separate the code that **reads data** from the code that **writes data**. Never mix them.

| | Command | Query |
|---|---|---|
| **Purpose** | Change something | Read something |
| **Returns** | The created/updated object | Data |
| **Examples** | `CreateProductCommand` | `GetProductsQuery` |
| **Side effects?** | Yes — writes to DB | No — never changes state |

### Why CQRS?

Without CQRS, controllers tend to grow into "god methods" that do everything:

```csharp
// WITHOUT CQRS — everything mixed together in the controller
[HttpPost]
public async Task<IActionResult> Create(ProductModel model)
{
    if (string.IsNullOrEmpty(model.Name)) return BadRequest("Name required");
    if (model.Price < 0) return BadRequest("Price must be positive");
    var entity = new Product { Name = model.Name, Price = model.Price };
    _context.Products.Add(entity);
    await _context.SaveChangesAsync();
    // ... what if we need to send an email? add inventory notification? log to audit?
    // all of that ends up here, making this method 200 lines long
    return Ok(entity);
}
```

With CQRS:

```csharp
// WITH CQRS — controller is 3 lines
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
{
    var product = await _mediator.Send(command, ct);
    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
}
```

The controller sends a command. MediatR routes it to the correct handler. The handler does the work. If you need to add audit logging later, you add a second handler or a pipeline behavior — the controller never changes.

### MediatR — The Message Bus

MediatR is the library that makes CQRS work in .NET. It is essentially a dictionary that maps:

```
GetProductsQuery  ──► GetProductsHandler
CreateProductCommand  ──► CreateProductHandler
```

When you call `_mediator.Send(new GetProductsQuery())`, MediatR:
1. Looks up the handler for `GetProductsQuery`
2. Runs any registered pipeline behaviors first (e.g., `ValidationBehavior`)
3. Calls `GetProductsHandler.Handle()`
4. Returns the result

### Full CQRS Flow for a GET request

```
Browser: GET https://localhost:5002/api/products
           + Authorization: Bearer <jwt>
    │
    ▼
ProductsController.GetAll()
    │   sends: new GetProductsQuery()
    ▼
ValidationBehavior (no validators registered for this query → passes through)
    │
    ▼
GetProductsHandler.Handle()
    │   calls: await _repository.GetAllAsync()
    ▼
ProductRepository.GetAllAsync()
    │   runs: SELECT * FROM Products ORDER BY CreatedAt DESC
    ▼
SQLite returns rows
    │
    ▼
Handler maps Product entities → List<ProductDto>
    │
    ▼
Controller returns: 200 OK + JSON array
    │
    ▼
Browser: displays the products table
```

### Full CQRS Flow for a POST request

```
Browser: POST https://localhost:5002/api/products
           + Authorization: Bearer <jwt>
           + Body: { "name": "Keyboard", "price": 149.99, ... }
    │
    ▼
ProductsController.Create()
    │   sends: CreateProductCommand("Keyboard", 149.99, ...)
    ▼
ValidationBehavior
    │   (runs any IValidator<CreateProductCommand> registered in DI)
    │   if invalid → throws ValidationException → returns 400 Bad Request
    │   if valid → continues
    ▼
CreateProductHandler.Handle()
    │   creates: new Product { Name = "Keyboard", Price = 149.99 }
    │   calls: await _repository.CreateAsync(product)
    ▼
ProductRepository.CreateAsync()
    │   runs: INSERT INTO Products (Name, Price, ...) VALUES (...)
    ▼
Returns the saved Product (with auto-generated Id)
    │
    ▼
Handler maps to ProductDto
    │
    ▼
Controller returns: 201 Created + Location header + ProductDto JSON
    │
    ▼
Vue adds the new product to the top of the list without page refresh
```

---

## 9. Database — Entity Framework Core + SQLite

**EF Core** is an ORM (Object Relational Mapper). You write C# classes and it generates and runs the SQL for you. You never write `SELECT` or `INSERT` statements manually.

**PostgreSQL** is a powerful, open-source relational database. It runs as a server process. In this project, the easiest way to run it is via the included `docker-compose.yml` which starts an official PostgreSQL 16 container with one command.

### Two separate databases

| Database | Used by | Contains |
|---|---|---|
| `inventory_db` | WebAPI | `Products` table |
| `identity_db` | IdentityServer | `AspNetUsers`, `AspNetRoles`, ASP.NET Identity tables |

They are completely separate. The WebAPI never touches user accounts, and IdentityServer never touches products.

### Database creation

Both databases are created **automatically on first startup** via EF Core Migrations:

- `identity_db` — `SeedData.EnsureSeedDataAsync()` calls `db.Database.MigrateAsync()` in IdentityServer's `Program.cs`
- `inventory_db` — WebAPI's `Program.cs` calls `db.Database.MigrateAsync()` on startup

EF Core connects to PostgreSQL, creates the database if it does not exist, runs all pending migrations, and seeds initial data. You do not need to run any migration commands manually.

---

## 10. Prerequisites

| Requirement | Version | Download |
|---|---|---|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Node.js | 20+ | https://nodejs.org/ |
| npm | 10+ | Included with Node.js |
| Docker Desktop | Latest | https://www.docker.com/products/docker-desktop/ |
| A modern browser | Any | Chrome, Firefox, Edge |

> **Note:** Docker is only needed to run the bundled PostgreSQL container. If you already have PostgreSQL 15+ installed locally, skip the Docker step and update the connection strings in `appsettings.json`.

### Check your versions

```bash
dotnet --version   # should print 10.x.x
node --version     # should print v20.x.x or higher
npm --version      # should print 10.x.x or higher
```

### Trust the HTTPS development certificate

The IdentityServer runs on HTTPS (`https://localhost:5001`). Your browser needs to trust the development certificate, otherwise it will show a security warning and the OIDC flow will fail.

```bash
dotnet dev-certs https --trust
```

On Windows this opens a dialog — click **Yes** to install the certificate.
On macOS it will ask for your password.
On Linux you may need to trust the certificate in your browser manually.

---

## 11. Running the Application

You need **3 separate terminal windows** running at the same time. They can all run in parallel — there is no order dependency, but IdentityServer must be running before you try to log in.

### Terminal 0 — Start PostgreSQL (Docker)

```bash
# from the project root (where docker-compose.yml lives)
docker compose up -d
```

This starts:
- **PostgreSQL 16** on `localhost:5432` — user: `postgres`, password: `postgres`
- **pgAdmin 4** on `http://localhost:5050` — login: `admin@admin.com` / `admin` (optional, for browsing the DB visually)

Wait a few seconds for Postgres to initialise, then proceed.

> **Custom credentials?** Edit the connection strings in both `appsettings.json` files and the environment variables in `docker-compose.yml` to match.

---

### Terminal 1 — IdentityServer (Authentication Server)

```bash
cd backend/src/IdentityServer/OAUTH_VUE_NET.IdentityServer
dotnet run
```

What happens on first run:
1. Connects to PostgreSQL at `localhost:5432`
2. Creates the `identity_db` database if it does not exist
3. Runs EF Core migrations (creates ASP.NET Identity tables)
4. Seeds test users: `alice` and `bob`
5. Starts listening on `https://localhost:5001`

You should see output like:
```
info: Now listening on: https://localhost:5001
info: Now listening on: http://localhost:5000
```

Verify it works: open `https://localhost:5001/.well-known/openid-configuration` in your browser — you should see a JSON document describing the IdentityServer endpoints.

---

### Terminal 2 — WebAPI (REST API)

```bash
cd backend/src/WebAPI/OAUTH_VUE_NET.WebAPI
dotnet run
```

What happens on first run:
1. Connects to PostgreSQL at `localhost:5432`
2. Creates the `inventory_db` database if it does not exist
3. Runs EF Core migrations (creates the `Products` table)
4. Seeds 3 sample products: Laptop, Wireless Mouse, Standing Desk
5. Starts listening on `https://localhost:5002`

Verify it works: open `https://localhost:5002/swagger` — you should see the Swagger UI. All endpoints require a Bearer token, so you cannot call them from Swagger without first getting a token.

---

### Terminal 3 — Vue.js Frontend

```bash
cd frontend
npm run dev
```

You should see:
```
  VITE v5.x.x  ready in xxx ms

  ➜  Local:   http://localhost:5173/
```

Open `http://localhost:5173` in your browser to use the application.

---

## 12. Test Credentials

These users are created automatically by `SeedData.cs` when IdentityServer starts.

| Username | Password  | Notes |
|----------|-----------|-------|
| alice    | Alice123! | Full access to all products |
| bob      | Bob123!   | Full access to all products |

---

## 13. API Reference

Base URL: `https://localhost:5002/api`

All endpoints require: `Authorization: Bearer <access_token>`

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|-------------|----------|
| `GET` | `/products` | List all products | — | `200 OK` + `ProductDto[]` |
| `GET` | `/products/{id}` | Get one product | — | `200 OK` + `ProductDto` or `404` |
| `POST` | `/products` | Create a product | `CreateProductCommand` JSON | `201 Created` + `ProductDto` |
| `PUT` | `/products/{id}` | Update a product | `UpdateProductRequest` JSON | `200 OK` + `ProductDto` or `404` |
| `DELETE` | `/products/{id}` | Delete a product | — | `204 No Content` or `404` |

### Example Request (Create Product)

```http
POST https://localhost:5002/api/products
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR...
Content-Type: application/json

{
  "name": "Mechanical Keyboard",
  "description": "Tactile switches, RGB backlight",
  "price": 149.99,
  "quantity": 25,
  "category": "Electronics"
}
```

### Example Response

```json
{
  "id": 4,
  "name": "Mechanical Keyboard",
  "description": "Tactile switches, RGB backlight",
  "price": 149.99,
  "quantity": 25,
  "category": "Electronics",
  "createdAt": "2026-02-19T17:30:00Z",
  "updatedAt": null
}
```

---

## 14. Common Problems & Fixes

### "Your connection is not private" in the browser

The HTTPS dev certificate is not trusted yet.

```bash
dotnet dev-certs https --trust
```

Then restart the browser.

---

### Products page shows a network error / 401

The WebAPI is returning `401 Unauthorized`. Possible reasons:

1. **IdentityServer is not running** — start it first (Terminal 1).
2. **Token has expired** — log out and log in again.
3. **CORS error** — make sure the frontend is running on `http://localhost:5173` (the exact URL configured in `Config.cs`).

---

### "Connection refused" or "could not connect to server"

PostgreSQL is not running. Start it with:

```bash
docker compose up -d
```

Then wait ~5 seconds and try again.

---

### IdentityServer login says "Invalid username or password"

Make sure you are typing:
- `alice` (lowercase) and `Alice123!` (capital A, ends with exclamation mark)
- `bob` (lowercase) and `Bob123!` (capital B, ends with exclamation mark)

If users were never created, check that `SeedData.EnsureSeedDataAsync()` ran without errors. You can delete `identity.db` and restart IdentityServer to re-run seeding from scratch.

---

### `dotnet run` fails with "Port 5001 is already in use"

Another process is using port 5001. Either stop that process or change the port in `Properties/launchSettings.json`.

---

### Vue app shows blank screen after login callback

Open the browser developer console (F12 → Console). Look for errors. Common cause: IdentityServer's HTTPS certificate is not trusted, so the token request is blocked by the browser.

---

### npm install fails

Make sure you have Node.js 20+ installed:
```bash
node --version
```
If the version is too old, download a newer one from https://nodejs.org/.

---

## 15. Glossary

| Term | Meaning |
|---|---|
| **SPA** | Single-Page Application — a web app that loads once and updates the page dynamically without full page refreshes |
| **JWT** | JSON Web Token — a signed string that encodes identity claims (who you are, what you're allowed to do). Looks like `xxxxx.yyyyy.zzzzz` |
| **OAuth 2.0** | An open standard that lets one application (Vue) get permission to access another application (WebAPI) on behalf of a user, without sharing the user's password |
| **OpenID Connect (OIDC)** | An identity layer on top of OAuth 2.0. Adds the concept of "who is this user" (not just "what can they access") |
| **Authorization Code Flow** | The most secure OAuth flow: the user logs in at the auth server, which gives back a short-lived *code*, which the client then exchanges for tokens |
| **PKCE** | Proof Key for Code Exchange — an extension to the Authorization Code flow that protects against interception attacks in public clients (like SPAs) that cannot store a client secret |
| **IdentityServer** | A self-hosted OAuth 2.0 / OIDC server (Duende IdentityServer). Acts like "Sign in with Google" but you run it yourself |
| **Bearer Token** | An access token sent in the `Authorization: Bearer <token>` HTTP header to prove identity to an API |
| **CQRS** | Command Query Responsibility Segregation — separating read operations (queries) from write operations (commands) |
| **MediatR** | A .NET library that implements the Mediator pattern. Routes commands/queries to their handlers |
| **EF Core** | Entity Framework Core — a .NET ORM that lets you work with databases using C# objects instead of raw SQL |
| **Repository Pattern** | Hiding all database logic behind an interface so the business logic never knows what database is used |
| **DTO** | Data Transfer Object — a simple class used to carry data between layers (avoids exposing internal database entities) |
| **DI / IoC** | Dependency Injection / Inversion of Control — instead of classes creating their own dependencies (`new ProductRepository()`), dependencies are provided from outside (injected by the framework). Makes testing easy |
| **Pinia** | The official Vue.js state management library. Stores shared state (like the current user) that any component can read |
| **Axios** | A JavaScript HTTP client library used to make API calls from the browser |
| **oidc-client-ts** | A TypeScript library that handles all the OAuth/OIDC browser-side plumbing: building redirect URLs, storing tokens, refreshing tokens |
| **PostgreSQL** | A powerful open-source relational database server. Used here via a Docker container |
| **Razor Pages** | An ASP.NET Core feature for server-rendered HTML pages. Used here only for the IdentityServer login page |
| **Seed Data** | Initial data inserted into a database when it is first created. Here: 3 sample products and 2 test users |
| **Middleware** | Code that runs in the ASP.NET Core HTTP pipeline for every request (authentication, CORS, etc.) |
| **CORS** | Cross-Origin Resource Sharing — a browser security feature. The WebAPI must explicitly allow requests from `http://localhost:5173`, otherwise the browser blocks them |
