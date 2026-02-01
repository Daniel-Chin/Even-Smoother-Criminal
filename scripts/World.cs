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

	// Counters for ID generation
	private int _nextIndividualId = 1;
	private int _nextFirmId = 1;
	private int _nextBankId = 1;

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
		sb.AppendLine("<table><thead><tr><th>ID</th><th>Name</th><th>Job</th><th>Employer</th><th class=\"money\">Balance</th></tr></thead><tbody>");
		foreach (var ind in Individuals)
		{
			var employer = ind.EmployerId != null ? GetFirm(ind.EmployerId) : null;
			var jobDisplay = ind.Job ?? "<span class=\"muted\">Unemployed</span>";
			var employerDisplay = employer != null ? E(employer.Name) : "<span class=\"muted\">-</span>";
			sb.AppendLine($"<tr><td>{E(ind.Id)}</td><td>{E(ind.Name)}</td>");
			sb.AppendLine($"<td>{jobDisplay}</td><td>{employerDisplay}</td>");
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
