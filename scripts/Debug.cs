using Godot;
using System;

public partial class Debug : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Print
		GD.Print("Debug node is ready.");

		// Set up an individual
		var individual = new Individual
		{
			Id = "indiv-001",
			Job = "Software Developer"
		};

		// Set up a firm with the individual as CEO
		var firm = new Firm
		{
			Id = "firm-001",
			CEO = individual,
			Employees = new System.Collections.Generic.List<Individual> { individual }
		};

		// Set up a transaction line
		var transactionLine = new TransactionLine
		{
			Id = "transline-001",
			Date = DateOnly.FromDateTime(DateTime.Now),
			Description = "Consulting Services",
			Amount = 5000
		};

		// Set up a firmsheet
		var firmSheet = new FirmSheet
		{
			Id = "firmsheet-001",
			AttatchedFirm = firm,
			Date = DateOnly.FromDateTime(DateTime.Now),
			Transactions = new System.Collections.Generic.List<TransactionLine> { transactionLine }
		};

		// Set up a bankstatement
		var bankStatement = new BankStatement
		{
			Id = "bankstmt-001",
			AttatchedIndividual = individual,
			Date = DateOnly.FromDateTime(DateTime.Now),
			Transactions = new System.Collections.Generic.List<TransactionLine> { transactionLine }
		};

		// Print out the created objects
		GD.Print($"Individual: {individual}");
		GD.Print($"Firm: {firm}");
		GD.Print($"Transaction Line: {transactionLine}");
		GD.Print($"Firm Sheet: {firmSheet}");
		GD.Print($"Bank Statement: {bankStatement}");

		// Print out the transactions from the bank statement
		foreach (var transaction in bankStatement.Transactions)
		{
			GD.Print($"Bank Statement Transaction: {transaction}");
		}


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
