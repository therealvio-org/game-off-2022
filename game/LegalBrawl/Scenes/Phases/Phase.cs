using Godot;

public class Phase : Node
{
    [Signal]
    public delegate void NextPhase(PhaseTypes nextPhase);
}