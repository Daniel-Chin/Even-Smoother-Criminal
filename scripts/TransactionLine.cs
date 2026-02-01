using System;
using System.Globalization;
using System.Net;

public record TransactionLine
{
	public DateOnly Date { get; init; }
	public string Description { get; init; }
	public int Amount { get; init; }
	public string Id { get; init; }

	private static string E(string s) => WebUtility.HtmlEncode(s ?? "");

	public static TransactionLine Example()
	{
		return new TransactionLine
		{
			Date = new DateOnly(2023, 1, 15),
			Description = "Office Supplies Purchase",
			Amount = 2500,
			Id = "TXN-1001"
		};
	}

	public string Render()
	{
		return $@"<div class=""transaction-line"">
  <div><strong>ID:</strong> {E(Id)}</div>
  <div><strong>Date:</strong> {Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}</div>
  <div><strong>Description:</strong> {E(Description)}</div>
  <div><strong>Amount:</strong> {Amount}</div>
</div>";
	}
}
