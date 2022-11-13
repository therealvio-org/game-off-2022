using Godot;

public class DamageCredibility : BaseCard
{
    [Export]
    public int Value = 7;
    public override string GetDescription() => $"Opponent loses {Value} #";
    public override void OnPlay()
    {
        ModifyCredibility(-Value, false);
    }
}