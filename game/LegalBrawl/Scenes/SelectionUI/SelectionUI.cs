using Godot;

public class SelectionUI : Control
{
    public Button RerollButton;
    public CardPool CardPool;

    public override void _Ready()
    {
        RerollButton = GetNode<Button>("RerollButton");
        CardPool = GetNode<CardPool>("CardPool");
    }

}