using System;

public record TransactionLine
{
	public DateOnly Date { get; init; }
	public string Description { get; init; }
	public int Amount { get; init; }
	public string Id { get; init; }


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
}
