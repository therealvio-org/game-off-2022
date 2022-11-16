using Godot;
using System;

public class TutorialView : View
{
    [Signal] public delegate void Finished();
    private Control _intro;
    private Control _selection;
    private Control _battle;
    private int _slide = 0;
    public override void _Ready()
    {
        base._Ready();
        _intro = GetNode<Control>("Overview");
        _selection = GetNode<Control>("Selection");
        _battle = GetNode<Control>("Battle");
    }

    public override void Setup()
    {
        _slide = 0;
        MouseFilter = MouseFilterEnum.Stop;
        Display(_slide);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == (int)ButtonList.Left && mouseButton.Pressed)
            {
                NextSlide();
            }
        }
    }

    public override void Cleanup()
    {
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void NextSlide()
    {
        if (++_slide >= 3)
        {
            EmitSignal("Finished");
            return;
        }
        Display(_slide);
    }

    public void Display(int slide)
    {
        _intro.Hide();
        _selection.Hide();
        _battle.Hide();

        switch (slide)
        {
            case 0: _intro.Show(); break;
            case 1: _selection.Show(); break;
            case 2: _battle.Show(); break;
        }
    }

}
