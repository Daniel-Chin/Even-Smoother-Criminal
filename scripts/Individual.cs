public record Individual: Entity 
{
	public string Job { get; init; }

	public static Individual Example()
	{
		return new Individual
		{
			Name = "Hanz von Salthole",
			Job = "Submarine Captain",
		};
	}

}
