using System;
using System.Collections.Generic;
using System.Text;
using System.Net; // added

public record IndividualBankStatement
{
    public record Transaction
    {
        public DateTime DateStamp { get; init; }
        public string Description { get; init; }
        public float Amount { get; init; }
    }

    public string CustomerName { get; init; }
    public string BankName { get; init; }
    public string BankAddress { get; init; }
    public string BankPhone { get; init; }
    public string BankWebsite { get; init; }
    public DateTime StatementPeriodStart { get; init; }
    public DateTime StatementPeriodEnd { get; init; }
    public DateTime StatementDate { get; init; }
    public string AccountType { get; init; }
    public string AccountNumber { get; init; }
    public string Currency { get; init; }
    public float BeginningBalance { get; init; }
    public List<Transaction> Transactions { get; init; } = [];

    private static string E(string s)
    {
        return WebUtility.HtmlEncode(s);
    }

    public string Render()
    {
        float credits = 0f;
        float debits  = 0f;
        float endingBalance = BeginningBalance;

        string maskedAccountNumber =
            AccountNumber?.Length > 4
                ? new string('*', AccountNumber.Length - 4) + AccountNumber[^4..]
                : (AccountNumber ?? string.Empty);

        const string styles = @"
<style>
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
</style>";

        StringBuilder transactionsHTML = new();
        foreach (var transaction in Transactions)
        {
            if (transaction.Amount > 0) credits += transaction.Amount;
            else debits += -transaction.Amount;

            endingBalance += transaction.Amount;

            var amountClass = transaction.Amount >= 0 ? "credit" : "debit";

            transactionsHTML.Append($@"    <tr>
        <td>{transaction.DateStamp.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant()}</td>
        <td>{E(transaction.Description)}</td>
        <td class=""num {amountClass}"">{transaction.Amount:F2}</td>
        <td class=""num"">{endingBalance:F2}</td>
    </tr>");
        }

        return $@"{styles}
<div class=""bank-statement"">
  <h2>{E(BankName)}</h2>
  <div class=""meta"">Customer: {E(CustomerName)}</div>
  <div class=""meta"">{E(BankAddress)} • {E(BankPhone)} • {E(BankWebsite)}</div>
  <div class=""meta"">
    Statement Period: {StatementPeriodStart.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant()}
    to {StatementPeriodEnd.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant()}
    &nbsp;&nbsp;&nbsp; Statement Date: {StatementDate.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant()}
  </div>
  <div class=""meta"">Account Type: {E(AccountType)} &nbsp;&nbsp;&nbsp; Account Number: {E(maskedAccountNumber)} &nbsp;&nbsp;&nbsp; Currency: {E(Currency)}</div>

  <div class=""rule""></div>

  <div class=""summary"">
    <div class=""row""><span class=""label"">Beginning Balance</span><span class=""value"">{BeginningBalance:C2}</span></div>
    <div class=""row""><span class=""label"">Ending Balance</span><span class=""value"">{endingBalance:C2}</span></div>
    <div class=""row""><span class=""label"">Credits</span><span class=""value"">{credits:C2}</span></div>
    <div class=""row""><span class=""label"">Debits</span><span class=""value"">-{debits:C2}</span></div>
  </div>

  <table class=""transactions"">
    <thead>
      <tr>
        <th style=""width:140px"">Date</th>
        <th>Description</th>
        <th style=""width:140px;text-align:right"">Amount</th>
        <th style=""width:140px;text-align:right"">Balance</th>
      </tr>
    </thead>
    <tbody>
{transactionsHTML}
    </tbody>
  </table>

  <div class=""foot"">This statement is provided for informational purposes only.</div>
</div>
";
    }

    public static IndividualBankStatement Example()
    {
        return new IndividualBankStatement
        {
            CustomerName = "John Doe",
            BankName = "Sample Bank",
            BankAddress = "123 Finance St, Money City, Country",
            BankPhone = "+1-800-555-1234",
            BankWebsite = "www.samplebank.com",
            StatementPeriodStart = new DateTime(2024, 1, 1),
            StatementPeriodEnd = new DateTime(2024, 1, 31),
            StatementDate = new DateTime(2024, 2, 1),
            AccountType = "Checking",
            AccountNumber = "1234567890",
            Currency = "USD",
            BeginningBalance = 1000.00f,
            Transactions =
            [
                new() { DateStamp = new DateTime(2024, 1, 5), Description = "Grocery Store", Amount = -150.75f },
                new() { DateStamp = new DateTime(2024, 1, 10), Description = "Salary Deposit", Amount = 2000.00f },
                new() { DateStamp = new DateTime(2024, 1, 15), Description = "Electric Bill", Amount = -75.50f },
                new() { DateStamp = new DateTime(2024, 1, 20), Description = "Restaurant", Amount = -60.00f },
                new() { DateStamp = new DateTime(2024, 1, 25), Description = "Gym Membership", Amount = -45.00f },
            ]
        };
    }
}
