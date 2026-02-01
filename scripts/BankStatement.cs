public record BankStatement: TransactionDoc
{
	public Individual AttatchedIndividual { get; init; }
}
