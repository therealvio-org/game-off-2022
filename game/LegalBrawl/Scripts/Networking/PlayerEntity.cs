using Godot;
using Godot.Collections;

public class PlayerEntity : Node
{
    public string PlayerId;
    public string PlayerName;
    public string Version;
    public int[] Cards;

    public PlayerEntity(Dictionary dict)
    {
        PlayerId = (string)dict["playerId"];
        PlayerName = (string)dict["playerName"];
        Version = (string)dict["version"];

        Array arr = (Array)dict["cards"];
        Cards = new int[arr.Count];
        for (int i = 0; i < arr.Count; i++)
        {
            Cards[i] = (int)((System.Single)arr[i]);
        }
    }
}