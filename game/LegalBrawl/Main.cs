using Godot;
using System;

public class Main : Control
{
    [Signal] delegate void BeginSelection();
    [Signal] delegate void BeginBattle();
    public Phase currentPhase;
    private GameUI _ui;
    public override void _Ready()
    {
        _ui = FindNode("GameUI") as GameUI;

        Connect("BeginSelection", this, "OnSelection");
        Connect("BeginBattle", this, "OnBattle");

        Debugger.Add("OnSelection", this);
    }

    public void OnSelection()
    {
        Phase selectionPhase = new Selection();
        BeginPhase(selectionPhase);
        _ui.Show(selectionPhase);
    }

    public void OnBattle()
    {
        Phase battlePhase = new Battle();
        BeginPhase(battlePhase);
        _ui.Show(battlePhase);
    }

    public void BeginPhase(Phase phase)
    {
        if (currentPhase != null)
            currentPhase.Cleanup();

        AddChild(phase);
        currentPhase = phase;
    }
}
