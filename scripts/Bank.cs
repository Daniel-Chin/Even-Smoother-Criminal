public record Bank: Firm
{
	public string Address { get; init; }
	public string Phone { get; init; }
	public string Website { get; init; }

	public new static Bank Example()
	{
		return new Bank
		{
			Name = "Sample Bank",
			Address = "123 Finance St, Money City, Country",
			Phone = "+1-800-555-1234",
			Website = "www.samplebank.com",
			CEO = Individual.Example(),
			Employees = new System.Collections.Generic.List<Individual>
			{
				Individual.Example(),
			},
		};
	}
}
