using Godot;
using System;

public class Menu : Phase
{
    public void ConnectTo(MenuView view)
    {
        view.Connect("Play", this, "OnPlay");
    }

    public void OnPlay()
    {
        EmitSignal("NextPhase", PhaseTypes.Selection);
    }
}