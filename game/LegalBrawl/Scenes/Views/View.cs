using Godot;
public class View : Control
{
    [Signal]
    public delegate void Enter();
    [Signal]
    public delegate void Exit();
    [Signal]
    public delegate void Activate();
    public override void _Ready()
    {
        Connect("Enter", this, "OnEnter");
        Connect("Exit", this, "OnExit");

        HideAllChildren();
    }

    public void OnEnter()
    {
        ShowAllChildren();
        Setup();
    }

    public virtual void Setup() { }

    public void OnExit()
    {
        HideAllChildren();
        Cleanup();
    }

    public virtual void Cleanup() { }

    public void HideAllChildren()
    {
        foreach (Node n in GetChildren())
        {
            if (n is Control c)
                c.Hide();
            if (n is CanvasLayer l)
                l.Hide();
        }
    }

    public void ShowAllChildren()
    {
        foreach (Node n in GetChildren())
        {
            if (n is Popup)
                continue;
            if (n is Control c)
                c.Show();
            if (n is CanvasLayer l)
                l.Show();
        }
    }
}