using Godot;

public struct CardPosition
{
    public Vector2 Position { get; private set; }
    public float Rotation { get; private set; }

    public CardPosition(Vector2 pos, float rot)
    {
        Position = pos;
        Rotation = rot;
    }
}