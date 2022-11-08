using Godot;
using System;

public class GameUI : Node
{
    private SelectionView _selectionView;
    //private BattleUI _battleUI;
    private View _currentView;
    private Phase _queuedPhase;
    private AnimationPlayer _animationPlayer;
    public override void _Ready()
    {
        _currentView = null;
        _selectionView = GetNode<SelectionView>("SelectionView");
        _animationPlayer = GetNode<AnimationPlayer>("CurtainTransition");
    }

    public void Transition(Phase phase)
    {
        _queuedPhase = phase;
        _animationPlayer.Play("Transition");
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
        if (phase is Selection selectionPhase)
            EnterSelection(selectionPhase);
    }

    public void EnterSelection(Selection selection)
    {
        _currentView = _selectionView;
        _selectionView.EmitSignal("Enter");
        selection.ConnectTo(_selectionView);
    }
}