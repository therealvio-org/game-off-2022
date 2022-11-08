using Godot;
using System;

public class GameUI : Node
{
    private SelectionUI _selectionUI;
    //private BattleUI _battleUI;
    public override void _Ready()
    {
        _selectionUI = GetNode<SelectionUI>("SelectionUI");
    }

    public void Show(Phase phase)
    {
        if (phase is Selection selectionPhase)
            ShowSelection(selectionPhase);
    }

    public void ShowSelection(Selection selection)
    {

        selection.ConnectTo(_selectionUI);
    }

}
