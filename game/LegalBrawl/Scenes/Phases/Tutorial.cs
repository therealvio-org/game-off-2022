using Godot;
using System;

public class Tutorial : Phase
{
    public void ConnectTo(TutorialView view)
    {
        view.Connect("Finished", this, "OnTutorialFinished");
    }

    public void OnTutorialFinished()
    {
        EmitSignal("NextPhase", PhaseTypes.Menu);
    }
}