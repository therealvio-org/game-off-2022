using Godot;
using System;

public class GameUI : Node
{
    private enum Views { None, Selection, Battle }
    private SelectionView _selectionView;
    //private BattleUI _battleUI;
    private Views _currentView;
    public override void _Ready()
    {
        _currentView = Views.None;
        _selectionView = GetNode<SelectionView>("SelectionView");
    }

    public void ExitCurrent()
    {
        switch (_currentView)
        {
            //case Selection: _selectionUI.EmitSignal("Exit");
        }
    }

    public void Show(Phase phase)
    {
        if (phase is Selection selectionPhase)
            ShowSelection(selectionPhase);
    }

    public void ShowSelection(Selection selection)
    {
        _selectionView.EmitSignal("Enter");
        selection.ConnectTo(_selectionView);
    }
}