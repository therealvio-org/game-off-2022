using Godot;
using System;

public class GameUI : Node
{
    private MenuView _menuView;
    private TutorialView _tutorialView;
    private SelectionView _selectionView;
    private NetworkingView _networkingView;
    private BattleView _battleView;
    private View _currentView;
    private Phase _queuedPhase;
    private AnimationPlayer _animationPlayer;
    public override void _Ready()
    {
        _currentView = null;
        _menuView = GetNode<MenuView>("MenuView");
        _tutorialView = GetNode<TutorialView>("TutorialView");
        _selectionView = GetNode<SelectionView>("SelectionView");
        _networkingView = GetNode<NetworkingView>("NetworkingView");
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

    public void ActivateCurrent()
    {
        _currentView.EmitSignal("Activate");
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

        if (phase is Tutorial tutorialPhase)
            EnterTutorial(tutorialPhase);

        if (phase is Selection selectionPhase)
            EnterSelection(selectionPhase);

        if (phase is Networking networkingPhase)
            EnterNetworking(networkingPhase);

        if (phase is Battle battlePhase)
            EnterBattle(battlePhase);
    }

    public void EnterMenu(Menu menu)
    {
        _currentView = _menuView;
        menu.ConnectTo(_menuView);
        _menuView.EmitSignal("Enter");
    }

    public void EnterTutorial(Tutorial tutorial)
    {
        _currentView = _tutorialView;
        _tutorialView.EmitSignal("Enter");
        tutorial.ConnectTo(_tutorialView);
    }

    public void EnterSelection(Selection selection)
    {
        _currentView = _selectionView;
        _selectionView.EmitSignal("Enter");
        selection.ConnectTo(_selectionView);
    }

    public void EnterNetworking(Networking networking)
    {
        _currentView = _networkingView;
        _networkingView.EmitSignal("Enter");
        networking.ConnectTo(_networkingView);
    }

    public void EnterBattle(Battle battle)
    {
        _currentView = _battleView;
        _battleView.EmitSignal("Enter");
        battle.ConnectTo(_battleView);
    }
}