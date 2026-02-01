using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

public record BankStatement: TransactionDoc
{
	public Individual AttatchedIndividual { get; init; }
	public Bank AttatchedBank { get; init; }
	public string AccountNumber { get; init; }
	public int BeginningBalance { get; init; }

	public static BankStatement Example()
	{
		return new BankStatement
		{
			AttatchedIndividual = Individual.Example(),
			AttatchedBank = Bank.Example(),
			Id = "BS-2023-01",
			Date = new DateOnly(2024, 1, 1),
			AccountNumber = "1234567890",
			BeginningBalance = 1000000,
			Transactions = new List<TransactionLine>
			{
				new() { Id = "TXN-1001", Date = new DateOnly(2024, 1, 5), Description = "Grocery Store", Amount = -150750 },
				new() { Id = "TXN-1002", Date = new DateOnly(2024, 1, 10), Description = "Salary Deposit", Amount = 2000000 },
				new() { Id = "TXN-1003", Date = new DateOnly(2024, 1, 15), Description = "Electric Bill", Amount = -75500 },
				new() { Id = "TXN-1004", Date = new DateOnly(2024, 1, 20), Description = "Restaurant", Amount = -60000 },
				new() { Id = "TXN-1005", Date = new DateOnly(2024, 1, 25), Description = "Gym Membership", Amount = -45000 },
			},
		};
	}

	private const decimal DisplayUnitDivisor = 1000m;

	private static string Money(decimal v)
	{
		var abs = Math.Abs(v).ToString("N2", CultureInfo.InvariantCulture);
		return v < 0 ? $"({abs})" : abs;
	}

	public override string Render()
	{
		decimal credits = 0m;
		decimal debits = 0m;
		decimal endingBalance = (decimal)BeginningBalance;

		string maskedAccountNumber =
			AccountNumber?.Length > 4
				? new string('*', AccountNumber.Length - 4) + AccountNumber[^4..]
				: (AccountNumber ?? string.Empty);

		var customerName = E(AttatchedIndividual?.Name ?? "");
		var periodStart = Date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture).ToLowerInvariant();
		var periodEndDate = Date.AddMonths(1).AddDays(-1);
		var periodEnd = periodEndDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture).ToLowerInvariant();
		var statementDate = periodEndDate.AddDays(1).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture).ToLowerInvariant();

		var sb = new StringBuilder(16_384);

		sb.AppendLine("<!doctype html>");
		sb.AppendLine("<html lang=\"en\">");
		sb.AppendLine("<head>");
		sb.AppendLine("<meta charset=\"utf-8\"/>");
		sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"/>");
		sb.AppendLine("<title>Bank Statement</title>");
		sb.AppendLine("<style>");
		sb.AppendLine(@"
.bank-statement{max-width:920px;margin:16px auto;padding:20px;border:1px solid #d7dbe2;border-radius:10px;background:#fff;
font:14px/1.45 system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#1f2937}
.bank-statement h2{margin:0 0 6px 0;font-size:20px;letter-spacing:.2px}
.bank-statement .meta{color:#4b5563;font-size:12.5px}
.bank-statement .rule{height:1px;background:#e5e7eb;margin:14px 0}
.bank-statement .summary{display:grid;grid-template-columns:1fr 1fr;gap:10px 18px;font-size:13px}
.bank-statement .summary .row{display:flex;justify-content:space-between;gap:12px;white-space:nowrap}
.bank-statement .summary .label{color:#6b7280}
.bank-statement .summary .value{font-variant-numeric:tabular-nums}
.bank-statement table{width:100%;border-collapse:collapse;margin-top:12px}
.bank-statement thead th{background:#0b3a6a;color:#fff;text-align:left;padding:10px 12px;font-weight:600;font-size:12.5px;letter-spacing:.2px}
.bank-statement tbody td{padding:10px 12px;border-bottom:1px solid #e5e7eb;vertical-align:top}
.bank-statement tbody tr:nth-child(even){background:#f9fafb}
.bank-statement .num{text-align:right;font-variant-numeric:tabular-nums}
.bank-statement .debit{color:#b91c1c}
.bank-statement .credit{color:#065f46}
.bank-statement .foot{margin-top:14px;color:#6b7280;font-size:12px}
");
		sb.AppendLine("</style>");
		sb.AppendLine("</head>");
		sb.AppendLine("<body>");

		sb.AppendLine("<div class=\"bank-statement\">");
		sb.AppendLine($"  <h2>{E(AttatchedBank?.Name ?? "")}</h2>");
		sb.AppendLine($"  <div class=\"meta\">Customer: {customerName}</div>");
		sb.AppendLine($"  <div class=\"meta\">{E(AttatchedBank?.Address ?? "")} • {E(AttatchedBank?.Phone ?? "")} • {E(AttatchedBank?.Website ?? "")}</div>");
		sb.AppendLine($"  <div class=\"meta\">Statement Period: {periodStart} to {periodEnd} &nbsp;&nbsp;&nbsp; Statement Date: {statementDate}</div>");
		sb.AppendLine($"  <div class=\"meta\">Account Type: Checking &nbsp;&nbsp;&nbsp; Account Number: {E(maskedAccountNumber)} &nbsp;&nbsp;&nbsp; Currency: USD</div>");
		sb.AppendLine("  <div class=\"rule\"></div>");

		// Build transactions HTML and calculate totals
		var transactionsHTML = new StringBuilder();
		foreach (var tx in Transactions ?? new List<TransactionLine>())
		{
			if (tx.Amount > 0) credits += tx.Amount;
			else debits += -tx.Amount;

			endingBalance += tx.Amount;

			var amountClass = tx.Amount >= 0 ? "credit" : "debit";
			var date = tx.Date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture).ToLowerInvariant();

			transactionsHTML.AppendLine("    <tr>");
			transactionsHTML.AppendLine($"      <td>{E(date)}</td>");
			transactionsHTML.AppendLine($"      <td>{E(tx.Description ?? "")}</td>");
			transactionsHTML.AppendLine($"      <td class=\"num {amountClass}\">{Money(((decimal)tx.Amount) / DisplayUnitDivisor)}</td>");
			transactionsHTML.AppendLine($"      <td class=\"num\">{Money(endingBalance / DisplayUnitDivisor)}</td>");
			transactionsHTML.AppendLine("    </tr>");
		}

		sb.AppendLine("  <div class=\"summary\">");
		sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Beginning Balance</span><span class=\"value\">{Money(((decimal)BeginningBalance) / DisplayUnitDivisor)}</span></div>");
		sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Ending Balance</span><span class=\"value\">{Money(endingBalance / DisplayUnitDivisor)}</span></div>");
		sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Credits</span><span class=\"value\">{Money(credits / DisplayUnitDivisor)}</span></div>");
		sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Debits</span><span class=\"value\">({Money(debits / DisplayUnitDivisor)})</span></div>");
		sb.AppendLine("  </div>");

		sb.AppendLine("  <table class=\"transactions\">");
		sb.AppendLine("    <thead>");
		sb.AppendLine("      <tr>");
		sb.AppendLine("        <th style=\"width:140px\">Date</th>");
		sb.AppendLine("        <th>Description</th>");
		sb.AppendLine("        <th style=\"width:140px;text-align:right\">Amount</th>");
		sb.AppendLine("        <th style=\"width:140px;text-align:right\">Balance</th>");
		sb.AppendLine("      </tr>");
		sb.AppendLine("    </thead>");
		sb.AppendLine("    <tbody>");
		sb.Append(transactionsHTML);
		sb.AppendLine("    </tbody>");
		sb.AppendLine("  </table>");

		sb.AppendLine("  <div class=\"foot\">This statement is provided for informational purposes only.</div>");
		sb.AppendLine("</div>");

		sb.AppendLine("</body>");
		sb.AppendLine("</html>");

		return sb.ToString();
	}
}
