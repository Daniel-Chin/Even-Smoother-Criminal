using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

public record TransactionDoc
{
	// Year-month. Always use the first day of the month.
	public DateOnly Date { get; init; }

	public List<TransactionLine> Transactions { get; init; }
	public string Id { get; init; }

	protected static string E(string s) => WebUtility.HtmlEncode(s ?? "");

	public virtual string Render()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"transaction-doc\">");
		sb.AppendLine($"  <div><strong>ID:</strong> {E(Id)}</div>");
		sb.AppendLine($"  <div><strong>Date:</strong> {Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}</div>");
		sb.AppendLine("  <div><strong>Transactions:</strong></div>");
		sb.AppendLine("  <ul>");
		foreach (var tx in Transactions ?? new List<TransactionLine>())
		{
			sb.AppendLine($"    <li>{tx.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}: {E(tx.Description)} ({tx.Amount})</li>");
		}
		sb.AppendLine("  </ul>");
		sb.AppendLine("</div>");
		return sb.ToString();
	}
}
