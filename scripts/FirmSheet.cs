using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Globalization;

public record FirmSheet: TransactionDoc
{
	public Firm AttatchedFirm { get; init; }
    public int BeginningAssets { get; init; }

	public static FirmSheet Example()
	{
		return new FirmSheet
		{
			AttatchedFirm = Firm.Example(),
			BeginningAssets = 50000,
			Id = "FS-2023-01",
			Date = new DateOnly(2023, 1, 1),
			Transactions = new List<TransactionLine>
			{
				TransactionLine.Example(),
				TransactionLine.Example(),
			},
		};
	}

	
    private const decimal DisplayUnitDivisor = 1000m;
	private static string E(string s)
    {
        return WebUtility.HtmlEncode(s);
    }

	
    private static string Money(decimal v)
    {
        // Strict: 2 decimals, parentheses for negatives, no currency symbol.
        var abs = Math.Abs(v).ToString("N2", CultureInfo.InvariantCulture);
        return v < 0 ? $"({abs})" : abs;
    }

	public string Render()
    {
        var firm = E(AttatchedFirm?.Name ?? "");
		var periodStart = Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		// Add a month to Date for period end
		var periodEndDate = Date.AddMonths(1).AddDays(-1);
		var periodEnd = periodEndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        // var unitLabel = E($"thousands of {Currency ?? ""}".Trim());
		// Worry about this later
        var unitLabel = E($"thousands of USD".Trim());

        decimal running = (decimal)BeginningAssets;
        decimal netChange = 0m;

        var sb = new StringBuilder(16_384);

        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\"/>");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"/>");
        sb.AppendLine("<title>Statement of Operations and Financial Position</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
:root { --ink:#111; --muted:#555; --rule:#bbb; }
* { box-sizing: border-box; }
body {
  margin: 32px;
  color: var(--ink);
  font: 14px/1.35 system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial, sans-serif;
}
.page { max-width: 920px; margin: 0 auto; }
.h1 { text-align: center; font-weight: 600; font-size: 18px; margin: 0 0 6px; }
.h2 { text-align: center; font-weight: 500; font-size: 14px; margin: 0 0 2px; color: var(--ink); }
.h3 { text-align: center; font-weight: 400; font-size: 12px; margin: 0 0 18px; color: var(--muted); }
.meta { display:flex; justify-content: space-between; gap: 12px; margin: 0 0 10px; color: var(--muted); font-size: 12px; }
table { width: 100%; border-collapse: collapse; }
th, td { padding: 6px 8px; vertical-align: top; }
thead th { border-bottom: 1px solid var(--rule); font-weight: 600; color: var(--ink); }
td.text, th.text { text-align: left; }
td.num, th.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
  font-feature-settings: ""tnum"" 1;
  white-space: nowrap;
}
tbody tr.section td { padding-top: 10px; }
tbody tr.rule-above td { border-top: 1px solid var(--rule); }
tbody tr.total td { font-weight: 700; }
.desc-muted { color: var(--muted); }
.footer { margin-top: 14px; color: var(--muted); font-size: 11px; }
");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"page\">");

        sb.AppendLine($"<div class=\"h1\">{firm}</div>");
        sb.AppendLine("<div class=\"h2\">Statement of Operations and Financial Position</div>");
        sb.AppendLine($"<div class=\"h2\">For the period {E(periodStart)} to {E(periodEnd)}</div>");
        sb.AppendLine($"<div class=\"h3\">All amounts in {unitLabel}</div>");

        sb.AppendLine("<table aria-label=\"Statement\">");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<th class=\"text\" style=\"width: 140px;\">Date</th>");
        sb.AppendLine("<th class=\"text\">Description</th>");
        sb.AppendLine($"<th class=\"num\" style=\"width: 170px;\">Amount</th>");
        sb.AppendLine($"<th class=\"num\" style=\"width: 190px;\">Assets</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");

        // Beginning assets
        sb.AppendLine("<tr class=\"section\">");
        sb.AppendLine("<td class=\"text\"><span class=\"desc-muted\">—</span></td>");
        sb.AppendLine("<td class=\"text\">Beginning assets</td>");
        sb.AppendLine("<td class=\"num\"></td>");
        sb.AppendLine($"<td class=\"num\">{Money(running / DisplayUnitDivisor)}</td>");
        sb.AppendLine("</tr>");

		foreach (var ev in Transactions ?? new List<TransactionLine>())
        {
            var date = ev.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var desc = E(ev.Description ?? "");
            netChange += (decimal)ev.Amount;
            running += (decimal)ev.Amount;

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td class=\"text\">{E(date)}</td>");
            sb.AppendLine($"<td class=\"text\">{desc}</td>");
            sb.AppendLine($"<td class=\"num\">{Money(((decimal)ev.Amount) / DisplayUnitDivisor)}</td>");
            sb.AppendLine($"<td class=\"num\">{Money(running / DisplayUnitDivisor)}</td>");
            sb.AppendLine("</tr>");
        }

        // Net change + ending assets
        sb.AppendLine("<tr class=\"rule-above\">");
        sb.AppendLine("<td class=\"text\"></td>");
        sb.AppendLine("<td class=\"text\">Net change</td>");
        sb.AppendLine($"<td class=\"num\">{Money(netChange / DisplayUnitDivisor)}</td>");
        sb.AppendLine("<td class=\"num\"></td>");
        sb.AppendLine("</tr>");

        sb.AppendLine("<tr class=\"rule-above total\">");
        sb.AppendLine("<td class=\"text\"></td>");
        sb.AppendLine("<td class=\"text\">Ending assets</td>");
        sb.AppendLine("<td class=\"num\"></td>");
        sb.AppendLine($"<td class=\"num\">{Money(running / DisplayUnitDivisor)}</td>");
        sb.AppendLine("</tr>");

        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        sb.AppendLine($"<div class=\"footer\">Generated {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} UTC</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
