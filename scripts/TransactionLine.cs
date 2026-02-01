using System;

public record TransactionLine
{
	public DateOnly Date { get; init; }
	public string Description { get; init; }
	public int Amount { get; init; }
	public string Id { get; init; }


}
