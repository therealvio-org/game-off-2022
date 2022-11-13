using Godot;
using System;

public class Main : Control
{
    [Signal] delegate void StartBattle();
    [Signal] delegate void StartSelection();
    [Signal] delegate void PhaseChange();
    public Phase CurrentPhase;
    private GameUI _ui;
    public override void _Ready()
    {
        _ui = FindNode("GameUI") as GameUI;

        Connect("PhaseChange", this, "OnPhaseChange");
        Connect("StartSelection", this, "GoToSelection");
        Connect("StartBattle", this, "GoToBattle");

        Debugger.Add("GoToSelection", this);
        Debugger.Add("GoToBattle", this);
    }

    public void GoToSelection()
    {
        EmitSignal("PhaseChange", new Selection());
    }

    public void GoToBattle()
    {
        if (CurrentPhase is Selection selectionPhase)
            EmitSignal("PhaseChange", new Battle(selectionPhase.GetHand(), CardLibrary.RandomHand()));
        else
            EmitSignal("PhaseChange", new Battle(CardLibrary.RandomHand(), CardLibrary.RandomHand()));
    }

    public void OnPhaseChange(Phase phase)
    {
        AddChild(phase);
        if (CurrentPhase != null)
            CurrentPhase.QueueFree();

        CurrentPhase = phase;
        _ui.Transition(phase);
    }
}
