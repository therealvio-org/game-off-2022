using Godot;

public class SelectionUI : Control
{
    [Signal]
    public delegate void DisplayCards(int[] cardIds, int handSize);
    [Signal]
    public delegate void AddCard(int id);
    [Signal]
    public delegate void RemoveCard(int id);

    public Button RerollButton;

    public override void _Ready()
    {
        RerollButton = GetNode<Button>("RerollButton");
    }

}