using Godot;
using System;

public class Menu : Phase
{
    [Signal] public delegate void GetPlayerName(bool queueSelection = false);
    public void ConnectTo(MenuView view)
    {
        view.Connect("Play", this, "OnPlay");
        view.Connect("Tutorial", this, "OnTutorial");
        Connect("GetPlayerName", view, "OnGetPlayerName");
    }

    public void OnPlay()
    {
        if (!GameStats.HasPlayerName())
        {
            EmitSignal("GetPlayerName", true);
            return;
        }

        EmitSignal("NextPhase", PhaseTypes.Selection);
    }
    public void OnTutorial()
    {
        EmitSignal("NextPhase", PhaseTypes.Tutorial);
    }
}