using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

public class WorldBrowser
{
	private readonly World _world;

	public WorldBrowser(World world)
	{
		_world = world;
	}

	private static string E(string s) => WebUtility.HtmlEncode(s ?? "");

	public string Route(string path)
	{
		// Parse path and query string
		var parts = path.Split('?', 2);
		var routePath = parts[0].Trim('/');
		var routeLower = routePath.ToLowerInvariant();
		var query = parts.Length > 1 ? ParseQuery(parts[1]) : new Dictionary<string, string>();

		// Match route (case-insensitive) but extract IDs with original case
		if (routeLower == "") return RenderDashboard();
		if (routeLower == "individuals") return RenderIndividualsList();
		if (routeLower.StartsWith("individual/")) return RenderIndividualDetail(routePath[11..]);
		if (routeLower == "firms") return RenderFirmsList();
		if (routeLower.StartsWith("firm/")) return RenderFirmDetail(routePath[5..]);
		if (routeLower == "banks") return RenderBanksList();
		if (routeLower.StartsWith("bank/")) return RenderBankDetail(routePath[5..]);
		if (routeLower == "documents") return RenderDocumentsList();
		if (routeLower.StartsWith("document/")) return RenderDocument(routePath[9..]);
		if (routeLower == "transactions") return RenderTransactions(query);

		return RenderNotFound(path);
	}

	private Dictionary<string, string> ParseQuery(string query)
	{
		var result = new Dictionary<string, string>();
		foreach (var pair in query.Split('&'))
		{
			var kv = pair.Split('=', 2);
			if (kv.Length == 2)
				result[WebUtility.UrlDecode(kv[0])] = WebUtility.UrlDecode(kv[1]);
		}
		return result;
	}

	private string WrapHtml(string title, string content, string activeNav = "")
	{
		return $@"<!doctype html>
<html><head>
<meta charset=""utf-8""/>
<title>{E(title)} - World Browser</title>
<style>
* {{ box-sizing: border-box; }}
body {{ font: 14px/1.5 system-ui, sans-serif; margin: 0; background: #f0f2f5; color: #1a1a1a; }}
.navbar {{ background: #1a365d; color: white; padding: 12px 20px; display: flex; gap: 20px; align-items: center; }}
.navbar a {{ color: white; text-decoration: none; padding: 6px 12px; border-radius: 4px; }}
.navbar a:hover {{ background: rgba(255,255,255,0.1); }}
.navbar a.active {{ background: rgba(255,255,255,0.2); }}
.navbar .title {{ font-weight: 600; font-size: 16px; margin-right: 20px; }}
.container {{ max-width: 1200px; margin: 20px auto; padding: 0 20px; }}
.card {{ background: white; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); padding: 20px; margin-bottom: 20px; }}
.card h2 {{ margin: 0 0 16px; font-size: 18px; color: #1a365d; }}
.card h3 {{ margin: 16px 0 12px; font-size: 15px; color: #2d3748; }}
table {{ width: 100%; border-collapse: collapse; }}
th, td {{ text-align: left; padding: 10px 12px; border-bottom: 1px solid #e2e8f0; }}
th {{ background: #f7fafc; font-weight: 600; font-size: 12px; text-transform: uppercase; color: #4a5568; }}
tr:hover {{ background: #f7fafc; }}
.money {{ text-align: right; font-variant-numeric: tabular-nums; }}
.positive {{ color: #22543d; }}
.negative {{ color: #c53030; }}
.muted {{ color: #718096; }}
a {{ color: #2b6cb0; text-decoration: none; }}
a:hover {{ text-decoration: underline; }}
.badge {{ display: inline-block; padding: 2px 8px; border-radius: 12px; font-size: 12px; font-weight: 500; }}
.badge-blue {{ background: #ebf8ff; color: #2b6cb0; }}
.badge-green {{ background: #f0fff4; color: #22543d; }}
.badge-gray {{ background: #edf2f7; color: #4a5568; }}
.stats {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 16px; margin-bottom: 20px; }}
.stat {{ background: white; border-radius: 8px; padding: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }}
.stat-value {{ font-size: 24px; font-weight: 600; color: #1a365d; }}
.stat-label {{ font-size: 12px; color: #718096; text-transform: uppercase; }}
.breadcrumb {{ margin-bottom: 16px; font-size: 13px; }}
.breadcrumb a {{ color: #4a5568; }}
.filter-bar {{ display: flex; gap: 12px; margin-bottom: 16px; flex-wrap: wrap; align-items: center; }}
.filter-bar select, .filter-bar input {{ padding: 6px 10px; border: 1px solid #e2e8f0; border-radius: 4px; font-size: 13px; }}
.btn {{ display: inline-block; padding: 8px 16px; background: #2b6cb0; color: white; border-radius: 4px; text-decoration: none; font-size: 13px; }}
.btn:hover {{ background: #2c5282; text-decoration: none; }}
</style>
</head>
<body>
<nav class=""navbar"">
	<span class=""title"">World Browser</span>
	<a href=""/"" class=""{(activeNav == "dashboard" ? "active" : "")}"">Dashboard</a>
	<a href=""/individuals"" class=""{(activeNav == "individuals" ? "active" : "")}"">Individuals</a>
	<a href=""/firms"" class=""{(activeNav == "firms" ? "active" : "")}"">Firms</a>
	<a href=""/banks"" class=""{(activeNav == "banks" ? "active" : "")}"">Banks</a>
	<a href=""/documents"" class=""{(activeNav == "documents" ? "active" : "")}"">Documents</a>
	<a href=""/transactions"" class=""{(activeNav == "transactions" ? "active" : "")}"">Transactions</a>
</nav>
<div class=""container"">
{content}
</div>
</body></html>";
	}

	private string RenderDashboard()
	{
		var sb = new StringBuilder();

		sb.AppendLine("<div class=\"stats\">");
		sb.AppendLine($"<div class=\"stat\"><div class=\"stat-value\">{_world.CurrentMonth:yyyy-MM}</div><div class=\"stat-label\">Current Month</div></div>");
		sb.AppendLine($"<div class=\"stat\"><div class=\"stat-value\">{_world.Individuals.Count}</div><div class=\"stat-label\">Individuals</div></div>");
		sb.AppendLine($"<div class=\"stat\"><div class=\"stat-value\">{_world.GetNonBankFirms().Count()}</div><div class=\"stat-label\">Firms</div></div>");
		sb.AppendLine($"<div class=\"stat\"><div class=\"stat-value\">{_world.GetBanks().Count()}</div><div class=\"stat-label\">Banks</div></div>");
		sb.AppendLine($"<div class=\"stat\"><div class=\"stat-value\">{_world.Transactions.Count}</div><div class=\"stat-label\">Transactions</div></div>");
		sb.AppendLine($"<div class=\"stat\"><div class=\"stat-value\">{_world.Documents.Count}</div><div class=\"stat-label\">Documents</div></div>");
		sb.AppendLine("</div>");

		// Recent transactions
		sb.AppendLine("<div class=\"card\"><h2>Recent Transactions</h2>");
		var recentTx = _world.Transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).Take(10);
		sb.AppendLine("<table><thead><tr><th>Date</th><th>From</th><th>To</th><th>Purpose</th><th class=\"money\">Amount</th></tr></thead><tbody>");
		foreach (var tx in recentTx)
		{
			var from = tx.FromEntityId != null ? _world.GetEntity(tx.FromEntityId)?.Name ?? tx.FromEntityId : "<span class=\"muted\">External</span>";
			var to = tx.ToEntityId != null ? _world.GetEntity(tx.ToEntityId)?.Name ?? tx.ToEntityId : "<span class=\"muted\">External</span>";
			sb.AppendLine($"<tr><td>{tx.Date:yyyy-MM-dd}</td><td>{from}</td><td>{to}</td><td><span class=\"badge badge-gray\">{tx.Purpose}</span></td><td class=\"money\">{tx.Amount:N0}</td></tr>");
		}
		sb.AppendLine("</tbody></table>");
		sb.AppendLine("<p><a href=\"/transactions\">View all transactions →</a></p></div>");

		return WrapHtml("Dashboard", sb.ToString(), "dashboard");
	}

	private string RenderIndividualsList()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"card\"><h2>Individuals</h2>");
		sb.AppendLine("<table><thead><tr><th>Name</th><th>Job</th><th>Employer</th><th>Bank</th><th class=\"money\">Balance</th><th>Statements</th></tr></thead><tbody>");

		foreach (var ind in _world.Individuals.OrderBy(i => i.Name))
		{
			var employer = ind.EmployerId != null ? _world.GetFirm(ind.EmployerId) : null;
			var bank = ind.BankId != null ? _world.GetBank(ind.BankId) : null;
			var stmtCount = _world.QueryBankStatements(individualId: ind.Id).Count();

			sb.AppendLine($"<tr>");
			sb.AppendLine($"<td><a href=\"/individual/{ind.Id}\">{E(ind.Name)}</a></td>");
			sb.AppendLine($"<td>{E(ind.Job) ?? "<span class=\"muted\">Unemployed</span>"}</td>");
			sb.AppendLine($"<td>{(employer != null ? $"<a href=\"/firm/{employer.Id}\">{E(employer.Name)}</a>" : "<span class=\"muted\">-</span>")}</td>");
			sb.AppendLine($"<td>{(bank != null ? $"<a href=\"/bank/{bank.Id}\">{E(bank.Name)}</a>" : "<span class=\"muted\">-</span>")}</td>");
			sb.AppendLine($"<td class=\"money\">{ind.Balance:N0}</td>");
			sb.AppendLine($"<td><span class=\"badge badge-blue\">{stmtCount}</span></td>");
			sb.AppendLine($"</tr>");
		}

		sb.AppendLine("</tbody></table></div>");
		return WrapHtml("Individuals", sb.ToString(), "individuals");
	}

	private string RenderIndividualDetail(string id)
	{
		var ind = _world.GetIndividual(id);
		if (ind == null) return RenderNotFound($"Individual {id}");

		var employer = ind.EmployerId != null ? _world.GetFirm(ind.EmployerId) : null;
		var bank = ind.BankId != null ? _world.GetBank(ind.BankId) : null;

		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"breadcrumb\"><a href=\"/individuals\">Individuals</a> / " + E(ind.Name) + "</div>");

		sb.AppendLine("<div class=\"card\">");
		sb.AppendLine($"<h2>{E(ind.Name)}</h2>");
		sb.AppendLine($"<p><strong>ID:</strong> {E(ind.Id)}</p>");
		sb.AppendLine($"<p><strong>Job:</strong> {E(ind.Job) ?? "<span class=\"muted\">Unemployed</span>"}</p>");
		sb.AppendLine($"<p><strong>Employer:</strong> {(employer != null ? $"<a href=\"/firm/{employer.Id}\">{E(employer.Name)}</a>" : "<span class=\"muted\">None</span>")}</p>");
		sb.AppendLine($"<p><strong>Bank:</strong> {(bank != null ? $"<a href=\"/bank/{bank.Id}\">{E(bank.Name)}</a>" : "<span class=\"muted\">None</span>")}</p>");
		sb.AppendLine($"<p><strong>Balance:</strong> <span class=\"money\">{ind.Balance:N0}</span></p>");
		sb.AppendLine("</div>");

		// Bank statements
		var statements = _world.QueryBankStatements(individualId: ind.Id).OrderByDescending(s => s.Date);
		sb.AppendLine("<div class=\"card\"><h2>Bank Statements</h2>");
		if (statements.Any())
		{
			sb.AppendLine("<table><thead><tr><th>Period</th><th>Bank</th><th class=\"money\">Beginning</th><th class=\"money\">Ending</th><th>Transactions</th><th></th></tr></thead><tbody>");
			foreach (var stmt in statements)
			{
				int ending = stmt.BeginningBalance + (stmt.Transactions?.Sum(t => t.Amount) ?? 0);
				sb.AppendLine($"<tr>");
				sb.AppendLine($"<td>{stmt.Date:yyyy-MM}</td>");
				sb.AppendLine($"<td>{E(stmt.AttatchedBank?.Name)}</td>");
				sb.AppendLine($"<td class=\"money\">{stmt.BeginningBalance:N0}</td>");
				sb.AppendLine($"<td class=\"money\">{ending:N0}</td>");
				sb.AppendLine($"<td><span class=\"badge badge-blue\">{stmt.Transactions?.Count ?? 0}</span></td>");
				sb.AppendLine($"<td><a href=\"/document/{stmt.Id}\" class=\"btn\">View</a></td>");
				sb.AppendLine($"</tr>");
			}
			sb.AppendLine("</tbody></table>");
		}
		else
		{
			sb.AppendLine("<p class=\"muted\">No statements yet.</p>");
		}
		sb.AppendLine("</div>");

		return WrapHtml(ind.Name, sb.ToString(), "individuals");
	}

	private string RenderFirmsList()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"card\"><h2>Firms</h2>");
		sb.AppendLine("<table><thead><tr><th>Name</th><th>CEO</th><th>Employees</th><th class=\"money\">Balance</th><th>Sheets</th></tr></thead><tbody>");

		foreach (var firm in _world.GetNonBankFirms().OrderBy(f => f.Name))
		{
			var ceo = firm.CeoId != null ? _world.GetIndividual(firm.CeoId) : null;
			var sheetCount = _world.QueryFirmSheets(firmId: firm.Id).Count();

			sb.AppendLine($"<tr>");
			sb.AppendLine($"<td><a href=\"/firm/{firm.Id}\">{E(firm.Name)}</a></td>");
			sb.AppendLine($"<td>{(ceo != null ? $"<a href=\"/individual/{ceo.Id}\">{E(ceo.Name)}</a>" : "<span class=\"muted\">-</span>")}</td>");
			sb.AppendLine($"<td>{firm.EmployeeIds?.Count ?? 0}</td>");
			sb.AppendLine($"<td class=\"money\">{firm.Balance:N0}</td>");
			sb.AppendLine($"<td><span class=\"badge badge-green\">{sheetCount}</span></td>");
			sb.AppendLine($"</tr>");
		}

		sb.AppendLine("</tbody></table></div>");
		return WrapHtml("Firms", sb.ToString(), "firms");
	}

	private string RenderFirmDetail(string id)
	{
		var firm = _world.GetFirm(id);
		if (firm == null) return RenderNotFound($"Firm {id}");

		var ceo = firm.CeoId != null ? _world.GetIndividual(firm.CeoId) : null;

		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"breadcrumb\"><a href=\"/firms\">Firms</a> / " + E(firm.Name) + "</div>");

		sb.AppendLine("<div class=\"card\">");
		sb.AppendLine($"<h2>{E(firm.Name)}</h2>");
		sb.AppendLine($"<p><strong>ID:</strong> {E(firm.Id)}</p>");
		sb.AppendLine($"<p><strong>CEO:</strong> {(ceo != null ? $"<a href=\"/individual/{ceo.Id}\">{E(ceo.Name)}</a>" : "<span class=\"muted\">None</span>")}</p>");
		sb.AppendLine($"<p><strong>Balance:</strong> <span class=\"money\">{firm.Balance:N0}</span></p>");

		// Employees
		sb.AppendLine("<h3>Employees</h3>");
		if (firm.EmployeeIds?.Count > 0)
		{
			sb.AppendLine("<ul>");
			foreach (var empId in firm.EmployeeIds)
			{
				var emp = _world.GetIndividual(empId);
				if (emp != null)
					sb.AppendLine($"<li><a href=\"/individual/{emp.Id}\">{E(emp.Name)}</a> - {E(emp.Job)}</li>");
			}
			sb.AppendLine("</ul>");
		}
		else
		{
			sb.AppendLine("<p class=\"muted\">No employees.</p>");
		}
		sb.AppendLine("</div>");

		// Firm sheets
		var sheets = _world.QueryFirmSheets(firmId: firm.Id).OrderByDescending(s => s.Date);
		sb.AppendLine("<div class=\"card\"><h2>Financial Statements</h2>");
		if (sheets.Any())
		{
			sb.AppendLine("<table><thead><tr><th>Period</th><th class=\"money\">Beginning Assets</th><th class=\"money\">Ending Assets</th><th>Transactions</th><th></th></tr></thead><tbody>");
			foreach (var sheet in sheets)
			{
				int ending = sheet.BeginningAssets + (sheet.Transactions?.Sum(t => t.Amount) ?? 0);
				sb.AppendLine($"<tr>");
				sb.AppendLine($"<td>{sheet.Date:yyyy-MM}</td>");
				sb.AppendLine($"<td class=\"money\">{sheet.BeginningAssets:N0}</td>");
				sb.AppendLine($"<td class=\"money\">{ending:N0}</td>");
				sb.AppendLine($"<td><span class=\"badge badge-green\">{sheet.Transactions?.Count ?? 0}</span></td>");
				sb.AppendLine($"<td><a href=\"/document/{sheet.Id}\" class=\"btn\">View</a></td>");
				sb.AppendLine($"</tr>");
			}
			sb.AppendLine("</tbody></table>");
		}
		else
		{
			sb.AppendLine("<p class=\"muted\">No statements yet.</p>");
		}
		sb.AppendLine("</div>");

		return WrapHtml(firm.Name, sb.ToString(), "firms");
	}

	private string RenderBanksList()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"card\"><h2>Banks</h2>");
		sb.AppendLine("<table><thead><tr><th>Name</th><th>Address</th><th>CEO</th><th>Customers</th><th class=\"money\">Balance</th></tr></thead><tbody>");

		foreach (var bank in _world.GetBanks().OrderBy(b => b.Name))
		{
			var ceo = bank.CeoId != null ? _world.GetIndividual(bank.CeoId) : null;
			var customerCount = _world.Individuals.Count(i => i.BankId == bank.Id);

			sb.AppendLine($"<tr>");
			sb.AppendLine($"<td><a href=\"/bank/{bank.Id}\">{E(bank.Name)}</a></td>");
			sb.AppendLine($"<td>{E(bank.Address)}</td>");
			sb.AppendLine($"<td>{(ceo != null ? $"<a href=\"/individual/{ceo.Id}\">{E(ceo.Name)}</a>" : "<span class=\"muted\">-</span>")}</td>");
			sb.AppendLine($"<td>{customerCount}</td>");
			sb.AppendLine($"<td class=\"money\">{bank.Balance:N0}</td>");
			sb.AppendLine($"</tr>");
		}

		sb.AppendLine("</tbody></table></div>");
		return WrapHtml("Banks", sb.ToString(), "banks");
	}

	private string RenderBankDetail(string id)
	{
		var bank = _world.GetBank(id);
		if (bank == null) return RenderNotFound($"Bank {id}");

		var ceo = bank.CeoId != null ? _world.GetIndividual(bank.CeoId) : null;
		var customers = _world.Individuals.Where(i => i.BankId == bank.Id).ToList();

		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"breadcrumb\"><a href=\"/banks\">Banks</a> / " + E(bank.Name) + "</div>");

		sb.AppendLine("<div class=\"card\">");
		sb.AppendLine($"<h2>{E(bank.Name)}</h2>");
		sb.AppendLine($"<p><strong>ID:</strong> {E(bank.Id)}</p>");
		sb.AppendLine($"<p><strong>Address:</strong> {E(bank.Address)}</p>");
		sb.AppendLine($"<p><strong>Phone:</strong> {E(bank.Phone)}</p>");
		sb.AppendLine($"<p><strong>Website:</strong> {E(bank.Website)}</p>");
		sb.AppendLine($"<p><strong>CEO:</strong> {(ceo != null ? $"<a href=\"/individual/{ceo.Id}\">{E(ceo.Name)}</a>" : "<span class=\"muted\">None</span>")}</p>");
		sb.AppendLine($"<p><strong>Balance:</strong> <span class=\"money\">{bank.Balance:N0}</span></p>");
		sb.AppendLine("</div>");

		// Customers
		sb.AppendLine("<div class=\"card\"><h2>Customers</h2>");
		if (customers.Any())
		{
			sb.AppendLine("<table><thead><tr><th>Name</th><th>Job</th><th class=\"money\">Balance</th><th></th></tr></thead><tbody>");
			foreach (var cust in customers.OrderBy(c => c.Name))
			{
				sb.AppendLine($"<tr>");
				sb.AppendLine($"<td><a href=\"/individual/{cust.Id}\">{E(cust.Name)}</a></td>");
				sb.AppendLine($"<td>{E(cust.Job) ?? "<span class=\"muted\">Unemployed</span>"}</td>");
				sb.AppendLine($"<td class=\"money\">{cust.Balance:N0}</td>");
				sb.AppendLine($"<td><a href=\"/individual/{cust.Id}\">View →</a></td>");
				sb.AppendLine($"</tr>");
			}
			sb.AppendLine("</tbody></table>");
		}
		else
		{
			sb.AppendLine("<p class=\"muted\">No customers.</p>");
		}
		sb.AppendLine("</div>");

		return WrapHtml(bank.Name, sb.ToString(), "banks");
	}

	private string RenderDocumentsList()
	{
		var sb = new StringBuilder();
		sb.AppendLine("<div class=\"card\"><h2>All Documents</h2>");
		sb.AppendLine("<table><thead><tr><th>ID</th><th>Type</th><th>Period</th><th>Entity</th><th></th></tr></thead><tbody>");

		foreach (var doc in _world.Documents.OrderByDescending(d => d.Date).ThenBy(d => d.Id))
		{
			string docType;
			string entityName;
			string entityLink;

			if (doc is BankStatement bs)
			{
				docType = "<span class=\"badge badge-blue\">Bank Statement</span>";
				entityName = bs.AttatchedIndividual?.Name ?? "Unknown";
				entityLink = bs.AttatchedIndividual != null ? $"/individual/{bs.AttatchedIndividual.Id}" : "#";
			}
			else if (doc is FirmSheet fs)
			{
				docType = "<span class=\"badge badge-green\">Firm Sheet</span>";
				entityName = fs.AttatchedFirm?.Name ?? "Unknown";
				entityLink = fs.AttatchedFirm != null ? $"/firm/{fs.AttatchedFirm.Id}" : "#";
			}
			else
			{
				docType = "<span class=\"badge badge-gray\">Document</span>";
				entityName = "Unknown";
				entityLink = "#";
			}

			sb.AppendLine($"<tr>");
			sb.AppendLine($"<td><code>{E(doc.Id)}</code></td>");
			sb.AppendLine($"<td>{docType}</td>");
			sb.AppendLine($"<td>{doc.Date:yyyy-MM}</td>");
			sb.AppendLine($"<td><a href=\"{entityLink}\">{E(entityName)}</a></td>");
			sb.AppendLine($"<td><a href=\"/document/{doc.Id}\" class=\"btn\">View</a></td>");
			sb.AppendLine($"</tr>");
		}

		sb.AppendLine("</tbody></table></div>");
		return WrapHtml("Documents", sb.ToString(), "documents");
	}

	private string RenderDocument(string id)
	{
		var doc = _world.Documents.FirstOrDefault(d => d.Id == id);
		if (doc == null) return RenderNotFound($"Document {id}");

		// Use the document's own Render method
		return doc.Render();
	}

	private string RenderTransactions(Dictionary<string, string> query)
	{
		query.TryGetValue("entity", out var entityFilter);
		query.TryGetValue("purpose", out var purposeFilter);
		query.TryGetValue("year", out var yearStr);
		query.TryGetValue("month", out var monthStr);

		int? year = int.TryParse(yearStr, out var y) ? y : null;
		int? month = int.TryParse(monthStr, out var m) ? m : null;
		TransactionPurpose? purpose = Enum.TryParse<TransactionPurpose>(purposeFilter, out var p) ? p : null;

		var transactions = _world.QueryTransactions(entityId: entityFilter, year: year, month: month, purpose: purpose)
			.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id);

		var sb = new StringBuilder();

		// Filter bar
		sb.AppendLine("<div class=\"card\">");
		sb.AppendLine("<form class=\"filter-bar\" method=\"get\">");
		sb.AppendLine("<label>Entity:</label>");
		sb.AppendLine("<select name=\"entity\"><option value=\"\">All</option>");
		foreach (var ind in _world.Individuals.OrderBy(i => i.Name))
			sb.AppendLine($"<option value=\"{ind.Id}\" {(entityFilter == ind.Id ? "selected" : "")}>{E(ind.Name)}</option>");
		foreach (var firm in _world.Firms.OrderBy(f => f.Name))
			sb.AppendLine($"<option value=\"{firm.Id}\" {(entityFilter == firm.Id ? "selected" : "")}>{E(firm.Name)}</option>");
		sb.AppendLine("</select>");

		sb.AppendLine("<label>Purpose:</label>");
		sb.AppendLine("<select name=\"purpose\"><option value=\"\">All</option>");
		foreach (var p2 in Enum.GetValues<TransactionPurpose>())
			sb.AppendLine($"<option value=\"{p2}\" {(purposeFilter == p2.ToString() ? "selected" : "")}>{p2}</option>");
		sb.AppendLine("</select>");

		sb.AppendLine("<label>Year:</label>");
		sb.AppendLine($"<input type=\"number\" name=\"year\" value=\"{yearStr}\" placeholder=\"e.g. 2026\" style=\"width:80px\"/>");

		sb.AppendLine("<label>Month:</label>");
		sb.AppendLine($"<input type=\"number\" name=\"month\" value=\"{monthStr}\" min=\"1\" max=\"12\" placeholder=\"1-12\" style=\"width:60px\"/>");

		sb.AppendLine("<button type=\"submit\" class=\"btn\">Filter</button>");
		sb.AppendLine("<a href=\"/transactions\">Clear</a>");
		sb.AppendLine("</form></div>");

		// Transaction list
		sb.AppendLine("<div class=\"card\"><h2>Transactions</h2>");
		sb.AppendLine("<table><thead><tr><th>ID</th><th>Date</th><th>From</th><th>To</th><th>Purpose</th><th>Description</th><th class=\"money\">Amount</th></tr></thead><tbody>");

		foreach (var tx in transactions.Take(100))
		{
			var fromEntity = tx.FromEntityId != null ? _world.GetEntity(tx.FromEntityId) : null;
			var toEntity = tx.ToEntityId != null ? _world.GetEntity(tx.ToEntityId) : null;

			var fromLink = fromEntity != null
				? $"<a href=\"/{(fromEntity is Individual ? "individual" : "firm")}/{fromEntity.Id}\">{E(fromEntity.Name)}</a>"
				: "<span class=\"muted\">External</span>";
			var toLink = toEntity != null
				? $"<a href=\"/{(toEntity is Individual ? "individual" : "firm")}/{toEntity.Id}\">{E(toEntity.Name)}</a>"
				: "<span class=\"muted\">External</span>";

			sb.AppendLine($"<tr>");
			sb.AppendLine($"<td class=\"muted\">{E(tx.Id)}</td>");
			sb.AppendLine($"<td>{tx.Date:yyyy-MM-dd}</td>");
			sb.AppendLine($"<td>{fromLink}</td>");
			sb.AppendLine($"<td>{toLink}</td>");
			sb.AppendLine($"<td><span class=\"badge badge-gray\">{tx.Purpose}</span></td>");
			sb.AppendLine($"<td>{E(tx.Description)}</td>");
			sb.AppendLine($"<td class=\"money\">{tx.Amount:N0}</td>");
			sb.AppendLine($"</tr>");
		}

		sb.AppendLine("</tbody></table>");
		var totalCount = transactions.Count();
		if (totalCount > 100)
			sb.AppendLine($"<p class=\"muted\">Showing 100 of {totalCount} transactions.</p>");
		sb.AppendLine("</div>");

		return WrapHtml("Transactions", sb.ToString(), "transactions");
	}

	private string RenderNotFound(string what)
	{
		return WrapHtml("Not Found", $"<div class=\"card\"><h2>Not Found</h2><p>Path: <code>{E(what)}</code></p><p>Length: {what?.Length ?? 0}</p><p>Bytes: {(what != null ? string.Join(" ", System.Text.Encoding.UTF8.GetBytes(what).Select(b => b.ToString("X2"))) : "null")}</p><p><a href=\"/\">← Back to Dashboard</a></p></div>");
	}
}
