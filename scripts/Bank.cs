using System.Collections.Generic;
using System.Text;

public record Bank: Firm
{
	public string Address { get; init; }
	public string Phone { get; init; }
	public string Website { get; init; }

	public new static Bank Example()
	{
		return new Bank
		{
			Id = "BANK-001",
			Name = "Sample Bank",
			Address = "123 Finance St, Money City, Country",
			Phone = "+1-800-555-1234",
			Website = "www.samplebank.com",
			CeoId = "IND-001",
			EmployeeIds = new List<string> { "IND-001" },
		};
	}

	public override string Render()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"bank\">");
		sb.AppendLine($"  <div><strong>ID:</strong> {E(Id)}</div>");
		sb.AppendLine($"  <div><strong>Name:</strong> {E(Name)}</div>");
		sb.AppendLine($"  <div><strong>Balance:</strong> {Balance}</div>");
		sb.AppendLine($"  <div><strong>Address:</strong> {E(Address)}</div>");
		sb.AppendLine($"  <div><strong>Phone:</strong> {E(Phone)}</div>");
		sb.AppendLine($"  <div><strong>Website:</strong> {E(Website)}</div>");
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
