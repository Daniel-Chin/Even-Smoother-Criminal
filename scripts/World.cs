using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

public partial class World : Node
{
	// World State
	public DateOnly CurrentMonth { get; private set; }
	public List<Individual> Individuals { get; private set; } = new();
	public List<Firm> Firms { get; private set; } = new();  // includes Banks
	public List<TransactionDoc> Documents { get; private set; } = new();
	public List<GroundTruthTransaction> Transactions { get; private set; } = new();  // ground truth history

	// Counters for ID generation
	private int _nextIndividualId = 1;
	private int _nextFirmId = 1;
	private int _nextBankId = 1;
	private int _nextTransactionId = 1;

	// Random for generation
	private Random _random;

	// Employment settings
	private int _minEmployeesPerFirm;
	private int _maxEmployeesPerFirm;

	public override void _Ready()
	{
	}

	public void Initialize(
		DateOnly startMonth,
		int numIndividuals,
		int numFirms,
		int numBanks,
		int minEmployeesPerFirm = 2,
		int maxEmployeesPerFirm = 10,
		int? seed = null)
	// Initializes the world with sample data
	// numFirms does not include banks
	{
		GD.Print("Initializing world...");
		GD.Print($"Start Month: {startMonth}");
		GD.Print($"Individuals: {numIndividuals}, Firms: {numFirms}, Banks: {numBanks}, Seed: {seed}");

		_random = seed.HasValue ? new Random(seed.Value) : new Random();
		_minEmployeesPerFirm = minEmployeesPerFirm;
		_maxEmployeesPerFirm = maxEmployeesPerFirm;

		CurrentMonth = startMonth;
		Individuals.Clear();
		Firms.Clear();
		Documents.Clear();
		Transactions.Clear();

		// Generate individuals
		for (int i = 0; i < numIndividuals; i++)
		{
			Individuals.Add(GenerateIndividual());
		}

		// Generate banks (a type of firm)
		for (int i = 0; i < numBanks; i++)
		{
			Firms.Add(GenerateBank());
		}

		// Generate regular firms
		for (int i = 0; i < numFirms; i++)
		{
			Firms.Add(GenerateFirm());
		}

		// Set up employment relationships
		AssignEmployment();

		// Assign bank accounts to individuals
		AssignBankAccounts();
	}

	private void AssignBankAccounts()
	{
		var banks = GetBanks().ToList();
		if (banks.Count == 0) return;

		// Distribute individuals across banks roughly evenly
		for (int i = 0; i < Individuals.Count; i++)
		{
			var ind = Individuals[i];
			var bank = banks[_random.Next(banks.Count)];
			ReplaceIndividual(ind.Id, ind with { BankId = bank.Id });
		}
	}

	private Individual GenerateIndividual()
	{
		string id = $"IND-{_nextIndividualId++:D4}";
		return new Individual
		{
			Id = id,
			Name = GeneratePersonName(),
			Balance = _random.Next(1000, 100000) * 100,  // 1k to 100k in cents
			Job = null,
			EmployerId = null,
		};
	}

	private Firm GenerateFirm()
	{
		string id = $"FIRM-{_nextFirmId++:D4}";
		return new Firm
		{
			Id = id,
			Name = GenerateCompanyName(),
			Balance = _random.Next(100000, 10000000) * 100,  // 100k to 10M in cents
			CeoId = null,
			EmployeeIds = new List<string>(),
		};
	}

	private Bank GenerateBank()
	{
		string id = $"BANK-{_nextBankId++:D4}";
		return new Bank
		{
			Id = id,
			Name = GenerateBankName(),
			Balance = _random.Next(10000000, 100000000) * 100,  // 10M to 100M in cents
			Address = GenerateAddress(),
			Phone = GeneratePhone(),
			Website = GenerateWebsite(),
			CeoId = null,
			EmployeeIds = new List<string>(),
		};
	}

	private void AssignEmployment()
	{
		if (Firms.Count == 0 || Individuals.Count == 0) return;

		var unemployed = new List<Individual>(Individuals);
		Shuffle(unemployed);

		var firmsList = Firms.ToList();
		int numFirms = firmsList.Count;
		int totalIndividuals = unemployed.Count;

		// Calculate fair distribution
		// Each firm needs at least 1 (CEO), plus additional employees
		int individualsPerFirm = totalIndividuals / numFirms;
		int remainder = totalIndividuals % numFirms;

		// Clamp to min/max range (min includes CEO, so actual additional = min-1)
		int targetPerFirm = Math.Clamp(individualsPerFirm, 1, _maxEmployeesPerFirm + 1);

		int unemployedIndex = 0;

		for (int firmIndex = 0; firmIndex < numFirms; firmIndex++)
		{
			var firm = firmsList[firmIndex];
			if (unemployedIndex >= unemployed.Count) break;

			// Assign CEO
			var ceo = unemployed[unemployedIndex++];
			var updatedCeo = ceo with { EmployerId = firm.Id, Job = "CEO" };
			ReplaceIndividual(ceo.Id, updatedCeo);

			var employeeIds = new List<string> { updatedCeo.Id };

			// Calculate employees for this firm (distribute remainder to first firms)
			int employeesForThisFirm = targetPerFirm;
			if (firmIndex < remainder) employeesForThisFirm++;

			// Add some variance if we have enough people, but stay within bounds
			int minForThisFirm = Math.Max(1, _minEmployeesPerFirm);
			int maxForThisFirm = Math.Min(employeesForThisFirm, _maxEmployeesPerFirm + 1);
			int actualEmployees = _random.Next(minForThisFirm, maxForThisFirm + 1);

			// Assign additional employees (excluding CEO already assigned)
			for (int i = 1; i < actualEmployees && unemployedIndex < unemployed.Count; i++)
			{
				var emp = unemployed[unemployedIndex++];
				var updatedEmp = emp with { EmployerId = firm.Id, Job = GenerateJobTitle() };
				ReplaceIndividual(emp.Id, updatedEmp);
				employeeIds.Add(updatedEmp.Id);
			}

			// Update firm with employee list
			ReplaceFirm(firm.Id, firm with { CeoId = updatedCeo.Id, EmployeeIds = employeeIds });
		}
	}

	private void ReplaceIndividual(string id, Individual updated)
	{
		int index = Individuals.FindIndex(i => i.Id == id);
		if (index >= 0) Individuals[index] = updated;
	}

	private void ReplaceFirm(string id, Firm updated)
	{
		int index = Firms.FindIndex(f => f.Id == id);
		if (index >= 0) Firms[index] = updated;
	}

	private void Shuffle<T>(List<T> list)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = _random.Next(i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	// Lookup helpers
	public Individual GetIndividual(string id) => Individuals.FirstOrDefault(i => i.Id == id);
	public Firm GetFirm(string id) => Firms.FirstOrDefault(f => f.Id == id);
	public Bank GetBank(string id) => Firms.OfType<Bank>().FirstOrDefault(b => b.Id == id);
	public IEnumerable<Bank> GetBanks() => Firms.OfType<Bank>();
	public IEnumerable<Firm> GetNonBankFirms() => Firms.Where(f => f is not Bank);
	public Entity GetEntity(string id) => (Entity)GetIndividual(id) ?? GetFirm(id);

	// Transaction queries
	public IEnumerable<GroundTruthTransaction> QueryTransactions(
		string entityId = null,
		int? year = null,
		int? month = null,
		TransactionPurpose? purpose = null)
	{
		IEnumerable<GroundTruthTransaction> result = Transactions;

		if (entityId != null)
			result = result.Where(t => t.FromEntityId == entityId || t.ToEntityId == entityId);

		if (year.HasValue)
			result = result.Where(t => t.Date.Year == year.Value);

		if (month.HasValue)
			result = result.Where(t => t.Date.Month == month.Value);

		if (purpose.HasValue)
			result = result.Where(t => t.Purpose == purpose.Value);

		return result.OrderBy(t => t.Date);
	}

	public IEnumerable<GroundTruthTransaction> GetTransactionsForEntity(string entityId)
		=> QueryTransactions(entityId: entityId);

	public IEnumerable<GroundTruthTransaction> GetTransactionsForMonth(int year, int month)
		=> QueryTransactions(year: year, month: month);

	// Document queries
	public IEnumerable<BankStatement> QueryBankStatements(
		string individualId = null,
		string bankId = null,
		int? year = null,
		int? month = null)
	{
		IEnumerable<BankStatement> result = Documents.OfType<BankStatement>();

		if (individualId != null)
			result = result.Where(s => s.AttatchedIndividual?.Id == individualId);

		if (bankId != null)
			result = result.Where(s => s.AttatchedBank?.Id == bankId);

		if (year.HasValue)
			result = result.Where(s => s.Date.Year == year.Value);

		if (month.HasValue)
			result = result.Where(s => s.Date.Month == month.Value);

		return result.OrderBy(s => s.Date);
	}

	public IEnumerable<FirmSheet> QueryFirmSheets(
		string firmId = null,
		int? year = null,
		int? month = null)
	{
		IEnumerable<FirmSheet> result = Documents.OfType<FirmSheet>();

		if (firmId != null)
			result = result.Where(s => s.AttatchedFirm?.Id == firmId);

		if (year.HasValue)
			result = result.Where(s => s.Date.Year == year.Value);

		if (month.HasValue)
			result = result.Where(s => s.Date.Month == month.Value);

		return result.OrderBy(s => s.Date);
	}

	public BankStatement GetBankStatement(string individualId, int year, int month)
		=> QueryBankStatements(individualId: individualId, year: year, month: month).FirstOrDefault();

	public FirmSheet GetFirmSheet(string firmId, int year, int month)
		=> QueryFirmSheets(firmId: firmId, year: year, month: month).FirstOrDefault();

	// Transaction recording
	public GroundTruthTransaction RecordTransaction(
		string fromEntityId,
		string toEntityId,
		int amount,
		TransactionPurpose purpose,
		string description,
		DateOnly? date = null)
	{
		var txDate = date ?? CurrentMonth;
		var tx = new GroundTruthTransaction
		{
			Id = $"TX-{_nextTransactionId++:D6}",
			Date = txDate,
			FromEntityId = fromEntityId,
			ToEntityId = toEntityId,
			Amount = amount,
			Purpose = purpose,
			Description = description,
		};
		Transactions.Add(tx);

		// Update balances (ground truth)
		if (fromEntityId != null)
		{
			var fromInd = GetIndividual(fromEntityId);
			if (fromInd != null)
				ReplaceIndividual(fromEntityId, fromInd with { Balance = fromInd.Balance - amount });
			else
			{
				var fromFirm = GetFirm(fromEntityId);
				if (fromFirm != null)
					ReplaceFirm(fromEntityId, fromFirm with { Balance = fromFirm.Balance - amount });
			}
		}

		if (toEntityId != null)
		{
			var toInd = GetIndividual(toEntityId);
			if (toInd != null)
				ReplaceIndividual(toEntityId, toInd with { Balance = toInd.Balance + amount });
			else
			{
				var toFirm = GetFirm(toEntityId);
				if (toFirm != null)
					ReplaceFirm(toEntityId, toFirm with { Balance = toFirm.Balance + amount });
			}
		}

		return tx;
	}

	// Monthly simulation
	public void AdvanceMonth()
	{
		CurrentMonth = CurrentMonth.AddMonths(1);
		GD.Print($"Advancing to {CurrentMonth:yyyy-MM}");

		ProcessPayroll();
		ProcessFirmTransactions();
		ProcessIndividualTransactions();
		GenerateMonthlyDocuments();
	}

	private void ProcessPayroll()
	{
		foreach (var firm in Firms.ToList())
		{
			foreach (var empId in firm.EmployeeIds ?? new List<string>())
			{
				var emp = GetIndividual(empId);
				if (emp == null) continue;

				// Salary based on job (CEO gets more)
				int salary = emp.Job == "CEO" ? _random.Next(8000, 15000) * 100 : _random.Next(3000, 8000) * 100;

				RecordTransaction(
					firm.Id,
					emp.Id,
					salary,
					TransactionPurpose.Salary,
					$"Salary payment to {emp.Name}",
					CurrentMonth);
			}
		}
	}

	private void ProcessFirmTransactions()
	{
		foreach (var firm in Firms.ToList())
		{
			// Operating expenses (1-5 per firm)
			int numExpenses = _random.Next(1, 6);
			for (int i = 0; i < numExpenses; i++)
			{
				int amount = _random.Next(1000, 50000) * 100;
				RecordTransaction(
					firm.Id,
					null,  // external party
					amount,
					TransactionPurpose.OperatingExpense,
					GenerateExpenseDescription(),
					RandomDateInMonth());
			}

			// Revenue from external (1-3 per firm)
			int numRevenue = _random.Next(1, 4);
			for (int i = 0; i < numRevenue; i++)
			{
				int amount = _random.Next(5000, 200000) * 100;
				RecordTransaction(
					null,  // external party
					firm.Id,
					amount,
					TransactionPurpose.Revenue,
					GenerateRevenueDescription(),
					RandomDateInMonth());
			}

			// Occasional firm-to-firm transaction (20% chance)
			if (_random.NextDouble() < 0.2)
			{
				var otherFirms = Firms.Where(f => f.Id != firm.Id).ToList();
				if (otherFirms.Count > 0)
				{
					var otherFirm = otherFirms[_random.Next(otherFirms.Count)];
					int amount = _random.Next(10000, 100000) * 100;
					RecordTransaction(
						firm.Id,
						otherFirm.Id,
						amount,
						TransactionPurpose.FirmToFirm,
						$"Business payment to {otherFirm.Name}",
						RandomDateInMonth());
				}
			}
		}
	}

	private void ProcessIndividualTransactions()
	{
		foreach (var ind in Individuals.ToList())
		{
			// Personal expenses (1-5 per individual)
			int numExpenses = _random.Next(1, 6);
			for (int i = 0; i < numExpenses; i++)
			{
				int amount = _random.Next(50, 2000) * 100;
				RecordTransaction(
					ind.Id,
					null,
					amount,
					TransactionPurpose.PersonalExpense,
					GeneratePersonalExpenseDescription(),
					RandomDateInMonth());
			}

			// Occasional personal income from external (10% chance, e.g., freelance)
			if (_random.NextDouble() < 0.1)
			{
				int amount = _random.Next(500, 5000) * 100;
				RecordTransaction(
					null,
					ind.Id,
					amount,
					TransactionPurpose.PersonalIncome,
					"Freelance income",
					RandomDateInMonth());
			}

			// Occasional individual-to-individual (5% chance)
			if (_random.NextDouble() < 0.05)
			{
				var others = Individuals.Where(i => i.Id != ind.Id).ToList();
				if (others.Count > 0)
				{
					var other = others[_random.Next(others.Count)];
					int amount = _random.Next(100, 1000) * 100;
					RecordTransaction(
						ind.Id,
						other.Id,
						amount,
						TransactionPurpose.IndividualToIndividual,
						$"Personal transfer to {other.Name}",
						RandomDateInMonth());
				}
			}
		}
	}

	private DateOnly RandomDateInMonth()
	{
		int day = _random.Next(1, DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month) + 1);
		return new DateOnly(CurrentMonth.Year, CurrentMonth.Month, day);
	}

	private static readonly string[] ExpenseCategories = { "Office supplies", "Utilities", "Rent", "Equipment", "Insurance", "Marketing", "Travel", "Maintenance", "Professional services", "Software licenses" };
	private static readonly string[] RevenueCategories = { "Product sales", "Service revenue", "Consulting fees", "Licensing income", "Contract payment", "Project milestone", "Subscription revenue" };
	private static readonly string[] PersonalExpenseCategories = { "Groceries", "Dining", "Transportation", "Entertainment", "Shopping", "Healthcare", "Utilities", "Subscription" };

	private string GenerateExpenseDescription() => ExpenseCategories[_random.Next(ExpenseCategories.Length)];
	private string GenerateRevenueDescription() => RevenueCategories[_random.Next(RevenueCategories.Length)];
	private string GeneratePersonalExpenseDescription() => PersonalExpenseCategories[_random.Next(PersonalExpenseCategories.Length)];

	// Document generation
	private int _nextDocId = 1;

	private void GenerateMonthlyDocuments()
	{
		// Generate bank statements for each individual
		foreach (var ind in Individuals)
		{
			var bank = GetBank(ind.BankId);
			if (bank == null) continue;

			// Get transactions involving this individual for this month
			var indTransactions = Transactions
				.Where(t => t.Date.Year == CurrentMonth.Year && t.Date.Month == CurrentMonth.Month)
				.Where(t => t.FromEntityId == ind.Id || t.ToEntityId == ind.Id)
				.OrderBy(t => t.Date)
				.ToList();

			// Calculate beginning balance (current balance minus net change this month)
			int netChange = indTransactions.Sum(t =>
				t.ToEntityId == ind.Id ? t.Amount : -t.Amount);
			int beginningBalance = ind.Balance - netChange;

			var transactionLines = indTransactions.Select(t => new TransactionLine
			{
				Id = $"TL-{_nextDocId++:D6}",
				Date = t.Date,
				Description = FormatTransactionDescription(t, ind.Id),
				Amount = t.ToEntityId == ind.Id ? t.Amount : -t.Amount,
			}).ToList();

			var statement = new BankStatement
			{
				Id = $"BS-{ind.Id}-{CurrentMonth:yyyyMM}",
				Date = CurrentMonth,
				AttatchedIndividual = ind,
				AttatchedBank = bank,
				AccountNumber = GenerateAccountNumber(ind.Id),
				BeginningBalance = beginningBalance,
				Transactions = transactionLines,
			};
			Documents.Add(statement);
		}

		// Generate firm sheets for each firm
		foreach (var firm in Firms)
		{
			// Get transactions involving this firm for this month
			var firmTransactions = Transactions
				.Where(t => t.Date.Year == CurrentMonth.Year && t.Date.Month == CurrentMonth.Month)
				.Where(t => t.FromEntityId == firm.Id || t.ToEntityId == firm.Id)
				.OrderBy(t => t.Date)
				.ToList();

			// Calculate beginning balance
			int netChange = firmTransactions.Sum(t =>
				t.ToEntityId == firm.Id ? t.Amount : -t.Amount);
			int beginningAssets = firm.Balance - netChange;

			var transactionLines = firmTransactions.Select(t => new TransactionLine
			{
				Id = $"TL-{_nextDocId++:D6}",
				Date = t.Date,
				Description = FormatTransactionDescription(t, firm.Id),
				Amount = t.ToEntityId == firm.Id ? t.Amount : -t.Amount,
			}).ToList();

			var sheet = new FirmSheet
			{
				Id = $"FS-{firm.Id}-{CurrentMonth:yyyyMM}",
				Date = CurrentMonth,
				AttatchedFirm = firm,
				BeginningAssets = beginningAssets,
				Transactions = transactionLines,
			};
			Documents.Add(sheet);
		}
	}

	private string FormatTransactionDescription(GroundTruthTransaction tx, string perspectiveEntityId)
	{
		// Format description from the perspective of the entity viewing the document
		bool isIncoming = tx.ToEntityId == perspectiveEntityId;
		string otherParty;

		if (isIncoming)
		{
			if (tx.FromEntityId == null)
				otherParty = "External";
			else
			{
				var from = GetEntity(tx.FromEntityId);
				otherParty = from?.Name ?? tx.FromEntityId;
			}
		}
		else
		{
			if (tx.ToEntityId == null)
				otherParty = "External";
			else
			{
				var to = GetEntity(tx.ToEntityId);
				otherParty = to?.Name ?? tx.ToEntityId;
			}
		}

		string direction = isIncoming ? "from" : "to";
		return $"{tx.Description} ({direction} {otherParty})";
	}

	private string GenerateAccountNumber(string entityId)
	{
		// Generate a deterministic account number from entity ID
		int hash = entityId.GetHashCode();
		return $"{Math.Abs(hash) % 10000000000:D10}";
	}

	// Debug render
	private static string E(string s) => WebUtility.HtmlEncode(s ?? "");

	public string Render()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"/>");
		sb.AppendLine("<style>");
		sb.AppendLine("body{font:14px/1.4 system-ui,sans-serif;margin:20px;background:#f5f5f5}");
		sb.AppendLine(".section{background:#fff;border:1px solid #ddd;border-radius:6px;padding:16px;margin-bottom:16px}");
		sb.AppendLine("h1{font-size:20px;margin:0 0 16px}h2{font-size:16px;margin:0 0 12px;border-bottom:1px solid #eee;padding-bottom:8px}");
		sb.AppendLine("table{width:100%;border-collapse:collapse;font-size:13px}");
		sb.AppendLine("th,td{text-align:left;padding:6px 8px;border-bottom:1px solid #eee}");
		sb.AppendLine("th{background:#f9f9f9;font-weight:600}");
		sb.AppendLine(".money{text-align:right;font-variant-numeric:tabular-nums}");
		sb.AppendLine(".muted{color:#666}");
		sb.AppendLine("</style></head><body>");

		sb.AppendLine("<div class=\"section\">");
		sb.AppendLine($"<h1>World State</h1>");
		sb.AppendLine($"<div><strong>Current Month:</strong> {CurrentMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture)}</div>");
		sb.AppendLine($"<div><strong>Individuals:</strong> {Individuals.Count} ({Individuals.Count(i => i.EmployerId != null)} employed)</div>");
		sb.AppendLine($"<div><strong>Firms:</strong> {GetNonBankFirms().Count()}</div>");
		sb.AppendLine($"<div><strong>Banks:</strong> {GetBanks().Count()}</div>");
		sb.AppendLine($"<div><strong>Transactions:</strong> {Transactions.Count}</div>");
		sb.AppendLine($"<div><strong>Documents:</strong> {Documents.Count}</div>");
		sb.AppendLine("</div>");

		// Banks
		sb.AppendLine("<div class=\"section\"><h2>Banks</h2>");
		sb.AppendLine("<table><thead><tr><th>ID</th><th>Name</th><th>Address</th><th>CEO</th><th>Employees</th><th class=\"money\">Balance</th></tr></thead><tbody>");
		foreach (var bank in GetBanks())
		{
			var ceo = GetIndividual(bank.CeoId);
			sb.AppendLine($"<tr><td>{E(bank.Id)}</td><td>{E(bank.Name)}</td><td>{E(bank.Address)}</td>");
			sb.AppendLine($"<td>{E(ceo?.Name ?? "-")}</td><td>{bank.EmployeeIds?.Count ?? 0}</td>");
			sb.AppendLine($"<td class=\"money\">{bank.Balance:N0}</td></tr>");
		}
		sb.AppendLine("</tbody></table></div>");

		// Firms
		sb.AppendLine("<div class=\"section\"><h2>Firms</h2>");
		sb.AppendLine("<table><thead><tr><th>ID</th><th>Name</th><th>CEO</th><th>Employees</th><th class=\"money\">Balance</th></tr></thead><tbody>");
		foreach (var firm in GetNonBankFirms())
		{
			var ceo = GetIndividual(firm.CeoId);
			sb.AppendLine($"<tr><td>{E(firm.Id)}</td><td>{E(firm.Name)}</td>");
			sb.AppendLine($"<td>{E(ceo?.Name ?? "-")}</td><td>{firm.EmployeeIds?.Count ?? 0}</td>");
			sb.AppendLine($"<td class=\"money\">{firm.Balance:N0}</td></tr>");
		}
		sb.AppendLine("</tbody></table></div>");

		// Individuals
		sb.AppendLine("<div class=\"section\"><h2>Individuals</h2>");
		sb.AppendLine("<table><thead><tr><th>ID</th><th>Name</th><th>Job</th><th>Employer</th><th>Bank</th><th class=\"money\">Balance</th></tr></thead><tbody>");
		foreach (var ind in Individuals)
		{
			var employer = ind.EmployerId != null ? GetFirm(ind.EmployerId) : null;
			var bank = ind.BankId != null ? GetBank(ind.BankId) : null;
			var jobDisplay = ind.Job ?? "<span class=\"muted\">Unemployed</span>";
			var employerDisplay = employer != null ? E(employer.Name) : "<span class=\"muted\">-</span>";
			var bankDisplay = bank != null ? E(bank.Name) : "<span class=\"muted\">-</span>";
			sb.AppendLine($"<tr><td>{E(ind.Id)}</td><td>{E(ind.Name)}</td>");
			sb.AppendLine($"<td>{jobDisplay}</td><td>{employerDisplay}</td><td>{bankDisplay}</td>");
			sb.AppendLine($"<td class=\"money\">{ind.Balance:N0}</td></tr>");
		}
		sb.AppendLine("</tbody></table></div>");

		sb.AppendLine("</body></html>");
		return sb.ToString();
	}

	// Name generation (placeholder - can be expanded)
	private static readonly string[] FirstNames = { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen" };
	private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
	private static readonly string[] CompanyPrefixes = { "Global", "United", "Premier", "Pacific", "Atlantic", "Northern", "Southern", "Central", "National", "American" };
	private static readonly string[] CompanySuffixes = { "Industries", "Corp", "Holdings", "Group", "Enterprises", "Solutions", "Partners", "Associates", "Consulting", "Services" };
	private static readonly string[] BankSuffixes = { "Bank", "Trust", "Financial", "Savings", "Credit Union" };
	private static readonly string[] JobTitles = { "Manager", "Analyst", "Engineer", "Director", "Accountant", "Consultant", "Specialist", "Coordinator", "Administrator", "Assistant" };

	private string GeneratePersonName() => $"{FirstNames[_random.Next(FirstNames.Length)]} {LastNames[_random.Next(LastNames.Length)]}";
	private string GenerateCompanyName() => $"{CompanyPrefixes[_random.Next(CompanyPrefixes.Length)]} {LastNames[_random.Next(LastNames.Length)]} {CompanySuffixes[_random.Next(CompanySuffixes.Length)]}";
	private string GenerateBankName() => $"{LastNames[_random.Next(LastNames.Length)]} {BankSuffixes[_random.Next(BankSuffixes.Length)]}";
	private string GenerateJobTitle() => JobTitles[_random.Next(JobTitles.Length)];
	private string GenerateAddress() => $"{_random.Next(1, 9999)} {LastNames[_random.Next(LastNames.Length)]} St, {CompanyPrefixes[_random.Next(CompanyPrefixes.Length)]} City";
	private string GeneratePhone() => $"+1-{_random.Next(200, 999)}-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}";
	private string GenerateWebsite() => $"www.{LastNames[_random.Next(LastNames.Length)].ToLower()}bank.com";
}
