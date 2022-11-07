using Godot;

public class AddCredibility : BaseCard
{
    [Export]
    public int Value = 5;

    public override void OnPlay()
    {
        _self.ModifyCredibility(Value);
    }
}