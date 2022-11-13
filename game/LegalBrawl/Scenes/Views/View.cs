using Godot;
public class View : Control
{
    [Signal]
    public delegate void Enter();
    [Signal]
    public delegate void Exit();
    public override void _Ready()
    {
        Connect("Enter", this, "OnEnter");
        Connect("Exit", this, "OnExit");

        HideAllChildren();
    }

    public void OnEnter()
    {
        ShowAllChildren();
    }

    public void OnExit()
    {
        HideAllChildren();
    }

    public void HideAllChildren()
    {
        foreach (Node n in GetChildren())
            if (n is Control c)
                c.Hide();
    }

    public void ShowAllChildren()
    {
        foreach (Node n in GetChildren())
            if (n is Control c)
                c.Show();
    }
}