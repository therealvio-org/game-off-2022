using Godot;
public class View : Control
{
    [Signal]
    public delegate void Enter();
    [Signal]
    public delegate void Exit();
    public override void _Ready()
    {

    }
}