using Godot;
using System;

public class Menu : Phase
{
    public void ConnectTo(MenuView view)
    {
        view.Connect("Play", this, "OnPlay");
        view.Connect("Tutorial", this, "OnTutorial");
    }

    public void OnPlay()
    {
        EmitSignal("NextPhase", PhaseTypes.Selection);
    }
    public void OnTutorial()
    {
        EmitSignal("NextPhase", PhaseTypes.Tutorial);
    }
}