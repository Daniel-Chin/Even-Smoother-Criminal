using System;
using System.Collections.Generic;

public record TransactionDoc
{
	// Year-month. Always use the first day of the month.
	public DateOnly Date { get; init; }

	public List<TransactionLine> Transactions { get; init; }
	public string Id { get; init; }


}
