using Godot;
using System;

public class Networking : Phase
{
    [Signal] public delegate void SendPlayerData();
    [Signal] public delegate void SendPlayerDataSuccess();
    [Signal] public delegate void SendPlayerDataFailure();
    [Signal] public delegate void GetOpponentData();
    [Signal] public delegate void GetOpponentDataSuccess();
    [Signal] public delegate void GetOpponentDataFailure();

    public override void _Ready()
    {

    }
    public void ConnectTo(NetworkingView view)
    {
        Connect("SendPlayerData", view, "OnSendPlayerData");
        Connect("SendPlayerDataSuccess", view, "OnSendPlayerDataSuccess");
        Connect("SendPlayerDataFailure", view, "OnSendPlayerDataFailure");
        Connect("GetOpponentData", view, "OnGetOpponentData");
        Connect("GetOpponentDataSuccess", view, "OnGetOpponentDataSuccess");
        Connect("GetOpponentDataFailure", view, "OnGetOpponentDataFailure");

        view.Connect("Activate", this, "OnViewActivate");
    }

    public void OnViewActivate()
    {
        PlayerEntity entity = new PlayerEntity(
            GameStats.GetId(),
            "Aidan Debug",
            Main.VERSION,
            HandCache.Get(PlayerTypes.Player).GetIds()
        );

        NetworkingManager.Instance.SendPlayerEntity(entity, this, "OnSendSuccess", "OnSendFailure");
        EmitSignal("SendPlayerData");
    }

    public void OnSendSuccess()
    {
        EmitSignal("SendPlayerDataSuccess");
    }

    public void OnSendFailure()
    {
        EmitSignal("SendPlayerDataFailure");
    }

}
