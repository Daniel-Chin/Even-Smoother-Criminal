public record Individual: Entity
{
	public string Job { get; init; }
	public string EmployerId { get; init; }  // null if unemployed
	public string BankId { get; init; }      // bank where this individual has their account

	public static Individual Example()
	{
		return new Individual
		{
			Id = "IND-001",
			Name = "Hanz von Salthole",
			Job = "Submarine Captain",
			BankId = "BANK-001",
		};
	}

	public override string Render()
	{
		return $@"<div class=""individual"">
  <div><strong>ID:</strong> {E(Id)}</div>
  <div><strong>Name:</strong> {E(Name)}</div>
  <div><strong>Balance:</strong> {Balance}</div>
  <div><strong>Job:</strong> {E(Job)}</div>
  <div><strong>Bank ID:</strong> {E(BankId)}</div>
</div>";
	}
}
