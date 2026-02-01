using System;

public enum TransactionPurpose
{
	Salary,
	OperatingExpense,
	Revenue,
	PersonalExpense,
	PersonalIncome,
	FirmToFirm,
	IndividualToIndividual,
	Other
}

public record GroundTruthTransaction
{
	public string Id { get; init; }
	public DateOnly Date { get; init; }
	public string FromEntityId { get; init; }  // null = external party
	public string ToEntityId { get; init; }    // null = external party
	public int Amount { get; init; }           // always positive, direction implied by From/To
	public TransactionPurpose Purpose { get; init; }
	public string Description { get; init; }   // human-readable description
}
