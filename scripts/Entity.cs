using System.Net;

public record Entity
{
	public string Id { get; init; }
	public string Name { get; init; }
	public int Balance { get; init; }

	protected static string E(string s) => WebUtility.HtmlEncode(s ?? "");

	public virtual string Render()
	{
		return $@"<div class=""entity"">
  <div><strong>ID:</strong> {E(Id)}</div>
  <div><strong>Name:</strong> {E(Name)}</div>
  <div><strong>Balance:</strong> {Balance}</div>
</div>";
	}
}
