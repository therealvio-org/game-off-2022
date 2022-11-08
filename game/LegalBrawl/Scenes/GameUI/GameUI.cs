using Godot;
using System;

public class GameUI : Node
{
    private enum Views { None, Selection, Battle }
    private SelectionUI _selectionUI;
    //private BattleUI _battleUI;
    private Views _currentView;
    public override void _Ready()
    {
        _currentView = Views.None;
        _selectionUI = GetNode<SelectionUI>("SelectionUI");
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
        _selectionUI.EmitSignal("Enter");
        selection.ConnectTo(_selectionUI);
    }
}