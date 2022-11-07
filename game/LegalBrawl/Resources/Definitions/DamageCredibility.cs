using Godot;

public class DamageCredibility : BaseCard
{
    [Export]
    public int Value = 7;

    public override void OnPlay()
    {
        _opponent.ModifyCredibility(Value);
    }
}