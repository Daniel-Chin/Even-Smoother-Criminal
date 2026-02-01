using Godot;
using System;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GD.Print("Main scene is ready.");
        BrowserDisplay browserDisplay = BrowserDisplay.AnyAvailable();
        browserDisplay.OpenBrowser("example.html");
    }
}
