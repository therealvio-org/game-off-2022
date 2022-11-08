using Godot;
using System;

public class Main : Control
{
    [Signal] delegate void PhaseChange();
    public Phase currentPhase;
    private GameUI _ui;
    public override void _Ready()
    {
        _ui = FindNode("GameUI") as GameUI;

        Connect("PhaseChange", this, "OnPhaseChange");

        Debugger.Add("GoToSelection", this);
    }

    public void GoToSelection()
    {
        EmitSignal("PhaseChange", new Selection());
    }

    public void OnPhaseChange(Phase phase)
    {
        AddChild(phase);
        currentPhase = phase;
        _ui.Transition(phase);
    }
}
