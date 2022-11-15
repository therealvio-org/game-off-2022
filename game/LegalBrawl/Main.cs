using Godot;
using System;

public class Main : Control
{
    [Signal] delegate void PhaseChange();
    public Phase CurrentPhase;
    private GameUI _ui;
    public override void _Ready()
    {
        _ui = FindNode("GameUI") as GameUI;

        Connect("PhaseChange", this, "OnPhaseChange");

        Debugger.Add("GoToSelection", this);
        Debugger.Add("GoToBattle", this);

        Intro();
    }

    public void Intro()
    {
        Phase menu = new Menu();
        SetPhase(menu);
        _ui.Enter(menu);
    }

    public void GoToMenu()
    {
        EmitSignal("PhaseChange", new Menu());
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
        SetPhase(phase);
        _ui.Transition(phase);
    }

    public void SetPhase(Phase phase)
    {
        AddChild(phase);
        if (CurrentPhase != null)
            CurrentPhase.QueueFree();

        phase.Connect("NextPhase", this, "OnNextPhase");

        CurrentPhase = phase;
    }

    public void OnNextPhase(PhaseTypes type)
    {
        if (type == PhaseTypes.Menu)
            GoToMenu();
        if (type == PhaseTypes.Selection)
            GoToSelection();
        if (type == PhaseTypes.Battle)
            GoToBattle();
    }
}
