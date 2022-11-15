using Godot;
using System;

public class GameUI : Node
{
    private MenuView _menuView;
    private SelectionView _selectionView;
    private BattleView _battleView;
    private View _currentView;
    private Phase _queuedPhase;
    private AnimationPlayer _animationPlayer;
    public override void _Ready()
    {
        _currentView = null;
        _menuView = GetNode<MenuView>("MenuView");
        _selectionView = GetNode<SelectionView>("SelectionView");
        _battleView = GetNode<BattleView>("BattleView");
        _animationPlayer = GetNode<AnimationPlayer>("CurtainTransition");
    }

    public void Transition(Phase phase, string transitionName = "Transition")
    {
        _queuedPhase = phase;
        _animationPlayer.Play(transitionName);
    }

    public void SwapPhases()
    {
        ExitCurrent();
        Enter(_queuedPhase);
    }

    public void ExitCurrent()
    {
        if (_currentView == null)
            return;

        _currentView.EmitSignal("Exit");
    }

    public void Enter(Phase phase)
    {
        if (phase is Menu menuPhase)
            EnterMenu(menuPhase);

        if (phase is Selection selectionPhase)
            EnterSelection(selectionPhase);

        if (phase is Battle battlePhase)
            EnterBattle(battlePhase);
    }

    public void EnterMenu(Menu menu)
    {
        _currentView = _menuView;
        _menuView.EmitSignal("Enter");
        menu.ConnectTo(_menuView);
    }

    public void EnterSelection(Selection selection)
    {
        _currentView = _selectionView;
        _selectionView.EmitSignal("Enter");
        selection.ConnectTo(_selectionView);
    }

    public void EnterBattle(Battle battle)
    {
        _currentView = _battleView;
        _battleView.EmitSignal("Enter");
        battle.ConnectTo(_battleView);
    }
}