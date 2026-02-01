# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Godot 4.6 game using C# (.NET 8.0) simulating financial activities and paper trails. The player is a financial fixer who investigates money trails or orchestrates money laundering.

**Core concept:** Turn-based simulation where entities conduct financial activities each month. Two versions of financial data exist:
- **Ground truth:** Actual balances and transaction history attached to entities (not visible to player)
- **Paper trail:** Transaction documents generated each month (may have discrepancies later)

## Build Commands

```bash
dotnet build slick.csproj
```

Run from Godot Editor. Opens in fullscreen borderless mode (1920x1080).

## Architecture

### Entity Hierarchy (C# Records, ID-based references)

```
Entity (Id, Name, Balance)
├── Individual (Job, EmployerId, BankId)
└── Firm (CeoId, EmployeeIds)
    └── Bank (Address, Phone, Website)
```

Relationships use string IDs for serialization safety:
- `Individual.EmployerId` → references `Firm.Id` (null if unemployed)
- `Individual.BankId` → references `Bank.Id` (bank account)
- `Firm.CeoId` → references `Individual.Id`
- `Firm.EmployeeIds` → list of `Individual.Id`

### Ground Truth Transactions

`GroundTruthTransaction` record tracks all actual money movement:
- `FromEntityId`, `ToEntityId` (null = external party)
- `Amount`, `Date`, `Purpose`, `Description`

`TransactionPurpose` enum: Salary, OperatingExpense, Revenue, PersonalExpense, PersonalIncome, FirmToFirm, IndividualToIndividual, Other

### Transaction Documents (Paper Trail)

```
TransactionDoc (Id, Date, Transactions)
├── BankStatement (AttatchedIndividual, AttatchedBank, AccountNumber, BeginningBalance)
└── FirmSheet (AttatchedFirm, BeginningAssets)
```

- `TransactionLine`: Date, Description, Amount, Id
- Documents generated at month end from ground truth transactions

### World State (`World.cs`)

Holds all simulation state:
- `CurrentMonth` (DateOnly)
- `Individuals`, `Firms` (includes Banks)
- `Transactions` (ground truth history)
- `Documents` (paper trail)

**Initialization:**
```csharp
world.Initialize(
    startMonth: new DateOnly(2026, 1, 1),
    numIndividuals: 50,
    numFirms: 5,
    numBanks: 2,
    minEmployeesPerFirm: 2,
    maxEmployeesPerFirm: 10,
    seed: 42);
```

**Monthly Simulation:**
```csharp
world.AdvanceMonth();  // Processes one month
```

`AdvanceMonth()` does:
1. Increments `CurrentMonth`
2. `ProcessPayroll()` - firms pay employees
3. `ProcessFirmTransactions()` - expenses, revenue, firm-to-firm
4. `ProcessIndividualTransactions()` - personal expenses, income, transfers
5. `GenerateMonthlyDocuments()` - creates BankStatements and FirmSheets

**Query Methods:**
```csharp
// Transactions
world.QueryTransactions(entityId, year, month, purpose);
world.GetTransactionsForEntity(entityId);

// Documents
world.QueryBankStatements(individualId, bankId, year, month);
world.QueryFirmSheets(firmId, year, month);
world.GetBankStatement(individualId, year, month);
world.GetFirmSheet(firmId, year, month);
```

### Browser Display System

`BrowserDisplay` runs a TCP server on localhost (ports 8000-8100).

```csharp
var browser = BrowserDisplay.AnyAvailable();

// Simple content
browser.SetContent(() => world.Render());

// With routing (for WorldBrowser)
browser.SetRouter(path => worldBrowser.Route(path));

browser.OpenBrowser("");
```

### WorldBrowser UI (`WorldBrowser.cs`)

Interactive browser-based UI for exploring world state.

**Routes:**
- `/` - Dashboard with stats and recent transactions
- `/individuals` - List all individuals
- `/individual/{id}` - Individual detail + bank statements
- `/firms` - List all firms
- `/firm/{id}` - Firm detail + employees + financial sheets
- `/banks` - List all banks
- `/bank/{id}` - Bank detail + customers
- `/documents` - List all document IDs
- `/document/{id}` - Render actual document
- `/transactions` - Transaction log with filters (entity, purpose, year, month)

**Usage:**
```csharp
var worldBrowser = new WorldBrowser(world);
browserDisplay.SetRouter(path => worldBrowser.Route(path));
```

**Routing note:** Route matching is case-insensitive, but entity IDs preserve original case (IDs are uppercase: `IND-0001`, `FIRM-0001`).

### Render Methods

All entities and documents have `Render()` methods returning HTML:
- `Entity`, `Individual`, `Firm`, `Bank` - simple attribute display
- `TransactionLine`, `TransactionDoc` - basic rendering
- `BankStatement`, `FirmSheet` - styled financial document output
- `World.Render()` - debug view showing all state

## Conventions

- Records use `Example()` factory methods for test data
- Amounts stored as integers (cents), displayed divided by 1000
- ID format: `IND-0001`, `FIRM-0001`, `BANK-0001`, `TX-000001`
- Use `with` keyword to create modified copies of records
- When iterating and modifying collections, use `.ToList()` to avoid enumeration errors
- `RecordTransaction()` automatically updates entity balances
