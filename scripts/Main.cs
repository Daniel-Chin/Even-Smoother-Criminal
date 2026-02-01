using Godot;
using System;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GD.Print("Main scene is ready.");
        BrowserDisplay browserDisplay = BrowserDisplay.AnyAvailable();

        // Initialize world
        World world = new World();
        world.Initialize(new DateOnly(2026, 1, 1), numIndividuals: 20, numFirms: 3, numBanks: 2, seed: 21);

        // Simulate a few months
        world.AdvanceMonth();
        world.AdvanceMonth();
        world.AdvanceMonth();

        // Set up browser with routing
        var browser = new WorldBrowser(world);
        browserDisplay.SetRouter(path => browser.Route(path));
        browserDisplay.OpenBrowser("");
    }
}
