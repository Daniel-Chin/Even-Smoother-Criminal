using System.Collections.Generic;
using System.Text;

public record Firm: Entity
{
	public List<string> EmployeeIds { get; init; }  // references to Individual.Id
	public string CeoId { get; init; }  // reference to Individual.Id

	public static Firm Example()
	{
		return new Firm
		{
			Id = "FIRM-001",
			Name = "Acme Corporation",
			CeoId = "IND-001",
			EmployeeIds = new List<string> { "IND-001", "IND-002" },
		};
	}

	public override string Render()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"firm\">");
		sb.AppendLine($"  <div><strong>ID:</strong> {E(Id)}</div>");
		sb.AppendLine($"  <div><strong>Name:</strong> {E(Name)}</div>");
		sb.AppendLine($"  <div><strong>Balance:</strong> {Balance}</div>");
		sb.AppendLine($"  <div><strong>CEO ID:</strong> {E(CeoId)}</div>");
		sb.AppendLine("  <div><strong>Employee IDs:</strong></div>");
		sb.AppendLine("  <ul>");
		foreach (var empId in EmployeeIds ?? new List<string>())
		{
			sb.AppendLine($"    <li>{E(empId)}</li>");
		}
		sb.AppendLine("  </ul>");
		sb.AppendLine("</div>");
		return sb.ToString();
	}
}
