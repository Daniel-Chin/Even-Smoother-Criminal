using System.Collections.Generic;

public record Firm: Entity 
{
	public List<Individual> Employees { get; init; }
	public Individual CEO { get; init; }

	public static Firm Example()
	{
		return new Firm
		{
			Name = "Acme Corporation",
			CEO = Individual.Example(),
			Employees = new List<Individual>
			{
				Individual.Example(),
				new Individual
				{
					Name = "Jane Doe",
					Job = "Chief Financial Officer",
				},
			},
		};
	}

}
