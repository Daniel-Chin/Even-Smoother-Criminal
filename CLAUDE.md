# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Godot 4.6 game using C# (.NET 8.0) that simulates financial document viewing. The game runs a local HTTP server to render financial statements (bank statements, firm sheets) in a browser.

## Build Commands

```bash
# Build the C# project
dotnet build slick.csproj

# Run from Godot Editor (or command line)
# The project opens in fullscreen borderless mode (1920x1080)
```

## Architecture

### Domain Model (C# Records)
- `Entity` - Base record with Id, Name, Balance
- `Individual` - Person entity with Job property
- `Firm` - Company with Employees list and CEO
- `TransactionLine` - Single transaction with Date, Description, Amount
- `TransactionDoc` - Base for documents containing transaction lists
- `BankStatement` - Individual's bank statement (extends TransactionDoc)
- `FirmSheet` - Company financial statement with HTML rendering capability

### Browser Display System
`BrowserDisplay` runs a lightweight TCP server (ports 8000-8100) that:
1. Listens for HTTP requests on localhost
2. Renders `FirmSheet` documents as HTML
3. Opens the system browser via `OS.ShellOpen()`

Entry point: `Main.cs` creates a BrowserDisplay and opens `example.html`.

### Conventions
- Domain objects use C# records with `Example()` factory methods for test data
- Amounts are stored as integers, displayed divided by 1000 (thousands)
- HTML output uses inline CSS with a clean financial document style
