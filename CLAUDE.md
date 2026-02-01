# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Godot 4.6 game using C# (.NET 8.0) simulating financial activities and paper trails. The player is a financial fixer who investigates money trails or orchestrates money laundering.

**Core concept:** Turn-based simulation where entities conduct financial activities each month. Two versions of financial data exist:
- **Ground truth:** Actual balances attached to entities
- **Paper trail:** Transaction documents that may not be truthful (e.g., discrepancies between months)

## Build Commands

```bash
dotnet build slick.csproj
```

Run from Godot Editor. Opens in fullscreen borderless mode (1920x1080).

## Architecture

### Entity Hierarchy (C# Records, ID-based references)

```
Entity (Id, Name, Balance)
├── Individual (Job, EmployerId)
└── Firm (CeoId, EmployeeIds)
    └── Bank (Address, Phone, Website)
```

Relationships use string IDs for serialization safety:
- `Individual.EmployerId` → references `Firm.Id` (null if unemployed)
- `Firm.CeoId` → references `Individual.Id`
- `Firm.EmployeeIds` → list of `Individual.Id`

### Transaction Documents

```
TransactionDoc (Id, Date, Transactions)
├── BankStatement (AttatchedIndividual, AttatchedBank, AccountNumber, BeginningBalance)
└── FirmSheet (AttatchedFirm, BeginningAssets)
```

- `TransactionLine`: Date, Description, Amount, Id

### World State (`World.cs`)

Holds all simulation state:
- `CurrentMonth` (DateOnly)
- `Individuals` (List)
- `Firms` (List, includes Banks)
- `Documents` (List, paper trail)

**Initialization:**
```csharp
world.Initialize(
    startMonth: new DateOnly(2026, 1, 1),
    numIndividuals: 50,
    numFirms: 5,
    numBanks: 2,
    minEmployeesPerFirm: 2,
    maxEmployeesPerFirm: 10,
    seed: 42);  // optional, for reproducibility
```

**Lookup helpers:** `GetIndividual(id)`, `GetFirm(id)`, `GetBank(id)`, `GetBanks()`, `GetNonBankFirms()`

Employment assignment distributes individuals evenly across firms when there aren't enough to fill all slots.

### Browser Display System

`BrowserDisplay` runs a TCP server on localhost (ports 8000-8100) to render HTML in browser.

```csharp
var browser = BrowserDisplay.AnyAvailable();
browser.SetContent(() => world.Render());  // Pass any object with Render()
browser.OpenBrowser("");
```

### Render Methods

All entities and documents have `Render()` methods returning HTML:
- `Entity`, `Individual`, `Firm`, `Bank` - simple attribute display
- `TransactionLine`, `TransactionDoc` - basic rendering
- `BankStatement`, `FirmSheet` - styled financial document output
- `World.Render()` - debug view showing all state

## Conventions

- Records use `Example()` factory methods for test data
- Amounts stored as integers (cents), displayed divided by 1000
- ID format: `IND-0001`, `FIRM-0001`, `BANK-0001`
- Use `with` keyword to create modified copies of records
- When iterating and modifying collections, use `.ToList()` to avoid enumeration errors
