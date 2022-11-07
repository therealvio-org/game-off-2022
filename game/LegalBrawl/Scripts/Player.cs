using Godot;

public class Player : Node
{
    private int _credibility;

    public void ModifyCredibility(int amount)
    {
        _credibility += amount;
    }
}