# WalletTracker 💳

A production-ready personal finance REST API built with **ASP.NET Core 8** and **Clean Architecture**. Manage multi-currency wallets, budgets, installment plans, and savings goals — backed by a Paymob-powered freemium subscription model.

---

## ✨ Features

- **Multi-wallet management** — Cash, Credit, and Pended balances per currency; peer-to-peer transfers
- **Transaction engine** — Income & expense recording with automatic budget sync and soft delete
- **Budget tracking** — Per-category spending limits with real-time `Spent` synchronization
- **Installment plans** — Monthly payment plans with completion tracking
- **Savings goals** — Incremental save-money flow with purchase completion
- **Analytics** — Dashboard, monthly trends, category breakdowns, budget health, upcoming installments
- **Subscriptions** — Paymob payment integration with HMAC-SHA512 webhook verification; Free / Premium tiers
- **Auth** — JWT login/signup, OTP password reset via email, profile management
- **Caching** — Redis cache-aside via a custom `[Cache]` action filter with configurable per-feature TTL
- **Rate limiting** — Global sliding-window + stricter fixed-window policies per route

---

## 🏗️ Architecture

```
WalletTracker/           ← ASP.NET Core host (DI wiring, middleware, config)
├── Domain/              ← Entities, contracts, exceptions, Money value object
├── Persistence/         ← EF Core DbContext, migrations, Generic Repository, Redis
│   └── Interceptors/    ← SoftDeleteInterceptor (transparent soft delete)
├── Service/             ← Business logic, AutoMapper profiles, ServiceManager
├── ServiceAbstraction/  ← Service interfaces and DTOs
├── Presentation/        ← Controllers, CacheAttribute, RequiresPremiumAttribute
└── Shared/              ← PagedResult<T>, error models
```

**Patterns used:** Clean Architecture · Repository + Unit of Work · Generic Repository · Soft Delete via EF Core interceptor · Cache-aside filter · Domain-owned financial mutations

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8 |
| Database | SQL Server + EF Core 8 |
| Identity | ASP.NET Core Identity + JWT Bearer |
| Cache | Redis (StackExchange.Redis) |
| Payment | Paymob Intention API |
| Email | MailKit / SMTP |
| Mapping | AutoMapper 12 |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)
- Redis (local — `redis-server` or Docker)
- Paymob account (optional, for subscription features)

### 1. Clone & restore

```bash
git clone https://github.com/your-username/WalletTracker.git
cd WalletTracker
dotnet restore
```

### 2. Configure

Edit `WalletTracker/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=WalletApp;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "WalletTracker",
    "Audience": "WalletTrackerUsers",
    "DurationInDays": 7
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "PaymobSettings": {
    "SecretKey": "...",
    "PublicKey": "...",
    "WebhookHmacSecret": "...",
    "IntegrationId": "...",
    "NotificationUrl": "https://your-domain.com/api/subscription/webhook",
    "RedirectionUrl": "https://your-frontend.com/payment/success"
  },
  "AdminCredentials": {
    "DefaultAdmin": {
      "Email": "admin@wallettracker.com",
      "Password": "Password123!",
      "FirstName": "Super",
      "LastName": "Admin"
    }
  }
}
```

### 3. Run

```bash
dotnet run --project WalletTracker
```

The app automatically runs pending migrations and seeds default categories and the admin user on startup. Open **https://localhost:7230/swagger** to explore the API.

---

## 📡 API Reference

| Group | Base route | Key endpoints |
|-------|-----------|---------------|
| Auth | `/api/authentication` | login, signup, forget-password, verify-otp, reset-password, update-user |
| Wallets | `/api/wallet` | CRUD, deposit, withdraw, transfer between wallets |
| Transactions | `/api/transaction` | CRUD with pagination per wallet |
| Budgets | `/api/budget` | CRUD with freemium limit enforcement |
| Categories | `/api/category` | CRUD (global seeded + user-owned) |
| Installments | `/api/installment` | CRUD + pay installment |
| Items to buy | `/api/itemtobuy` | CRUD + save-money + complete purchase |
| Analytics | `/api/analytics` | dashboard, category-spending, monthly-trends |
| Subscriptions | `/api/subscription` | initiate, webhook, status |

---

## 💎 Freemium Limits

| Feature | Free | Premium |
|---------|:----:|:-------:|
| Wallets | 1 | Unlimited |
| Budgets | 3 | Unlimited |
| Items to buy | 5 | Unlimited |

Premium is unlocked via Paymob (6-month subscription). The `[RequiresPremium]` action filter enforces premium-only endpoints.

---

## 📁 Seeded Data

On first run the app seeds:

- **5 default categories** — Food & Drinks, Transportation, Shopping, Housing & Utilities, Entertainment
- **Admin user** — credentials from `appsettings.json > AdminCredentials`
- **Default EGP wallet** — created automatically for every new user on signup


