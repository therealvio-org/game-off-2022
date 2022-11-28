using Godot;
using System;

public class Main : Control
{
    public const string VERSION = "1.0";
    public const int MAX_HAND = 7;
    public const int MAX_POOL = 10;
    public const int STARTING_FUNDS = 100;
    public const int CREDIBILITY = 15;
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
        AudioManager.PlayMusic("Menu");
        EmitSignal("PhaseChange", new Menu());
    }

    public void GoToTutorial()
    {
        EmitSignal("PhaseChange", new Tutorial());
    }

    public void GoToSelection()
    {
        AudioManager.PlayMusic("Selection");
        EmitSignal("PhaseChange", new Selection());
    }

    public void GoToNetworking()
    {
        AudioManager.PlayMusic("Battle");
        EmitSignal("PhaseChange", new Networking());
    }

    public void GoToBattle()
    {
        //AudioManager.PlayMusic("BattleIntro");
        EmitSignal("PhaseChange", new Battle());
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
        if (type == PhaseTypes.Tutorial)
            GoToTutorial();
        if (type == PhaseTypes.Selection)
            GoToSelection();
        if (type == PhaseTypes.Networking)
            GoToNetworking();
        if (type == PhaseTypes.Battle)
            GoToBattle();
    }
}
