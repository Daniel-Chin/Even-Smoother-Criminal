using System.Collections.Generic;

public record Firm: Entity 
{
	public List<Individual> Employees { get; init; }
	public Individual CEO { get; init; }


}
