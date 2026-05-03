# RentalApp — Library of Things

A peer-to-peer rental marketplace built with .NET MAUI, PostgreSQL, and Entity Framework Core for SET09102.

## Overview

RentalApp lets users list items they own, browse available items, and request rentals. Owners can approve or reject incoming requests and mark items as returned. The app follows MVVM architecture with a repository pattern and a dedicated service layer.

## Tech Stack

- **.NET MAUI** (net10.0-android) — cross-platform UI
- **PostgreSQL 16** via Docker Compose
- **Entity Framework Core 10** (Npgsql provider) — ORM + migrations
- **BCrypt.Net** — password hashing
- **CommunityToolkit.Mvvm** — ObservableObject, RelayCommand
- **xUnit + Moq** — unit and integration tests

## Project Structure

```
RentalApp/               # MAUI application (Views, ViewModels, Services, Converters)
RentalApp.Database/      # Class library: Models, DbContext, Repositories, RentalService
RentalApp.Migrations/    # Console app: runs EF Core migrations
RentalApp.Test/          # xUnit test project
docker-compose.yml       # PostgreSQL + pgAdmin
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- Android emulator (API 35+) or physical device
- ADB installed and on PATH

### 1. Start the database

```bash
docker compose up -d
```

### 2. Run migrations

```bash
CONNECTION_STRING="Host=localhost;Username=app_user;Password=app_password;Database=appdb" \
  dotnet run --project RentalApp.Migrations/RentalApp.Migrations.csproj
```

### 3. Deploy to Android emulator

Start ADB on the host so the emulator can be reached:

```bash
adb -a -P 5037 nodaemon server start
```

Then build and install:

```bash
dotnet build RentalApp/RentalApp.csproj -c Debug -f net10.0-android
adb install -r RentalApp/bin/Debug/net10.0-android/com.companyname.rentalapp-Signed.apk
```

The app connects to the database via `10.0.2.2:5432` (Android emulator's alias for the host machine).

## Running Tests

```bash
dotnet test RentalApp.Test/RentalApp.Test.csproj --verbosity normal
```

The test project uses:
- **`IClassFixture<DatabaseFixture>`** — spins up an EF Core in-memory database seeded with categories, users, and items
- **`Moq`** — mocks repositories for service-layer unit tests
- **`[Theory] / [InlineData]`** — data-driven tests for multiple scenarios

## Architecture

### MVVM Pattern

ViewModels inherit `BaseViewModel` (ObservableObject) and expose `[ObservableProperty]` fields and `[RelayCommand]` methods. Views bind to these via XAML. Business logic lives in services, not ViewModels.

### Repository Pattern

`IRepository<T>` defines `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`. Domain-specific interfaces (`IItemRepository`, `IRentalRepository`, `IReviewRepository`) extend it with queries like `GetByOwnerAsync` and `GetByCategoryAsync`.

### Service Layer

`RentalService` encapsulates booking logic: overlap detection, price calculation, and status-transition guards. It depends only on repository interfaces, making it fully testable with mocks.

### Authentication

`LocalAuthenticationService` uses BCrypt to hash passwords and stores users in PostgreSQL. It exposes `IsAuthenticated`, `CurrentUser`, and role-checking helpers (`HasRole`, `HasAnyRole`).

## CI/CD

GitHub Actions runs on every push to `main`/`develop` and on pull requests to `main`. The pipeline:

1. Starts a PostgreSQL 16 service container
2. Restores, builds, and runs the test suite with code coverage
3. Runs EF Core migrations against the CI database

See [.github/workflows/build.yml](.github/workflows/build.yml).
