using Godot;

public class AddCredibility : BaseCard
{
    [Export]
    public int Value = 5;
    public override string GetDescription() => $"Gain {Value} #";

    public override void OnPlay()
    {
        ModifyCredibility(Value);
    }
}