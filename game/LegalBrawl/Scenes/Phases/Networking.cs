using Godot;
using System;

public class Networking : Phase
{
    [Signal] public delegate void SendPlayerData();
    [Signal] public delegate void GetOpponentData();
    [Signal] public delegate void BattleReady();

    public override void _Ready()
    {
        Connect("SendPlayerData", this, "OnSendPlayerData");
        Connect("GetOpponentData", this, "OnGetOpponentData");
        Connect("BattleReady", this, "OnBattleReady");
    }
    public void ConnectTo(NetworkingView view)
    {
        Connect("SendPlayerData", view, "OnSendPlayerData");
        Connect("GetOpponentData", view, "OnGetOpponentData");

        view.Connect("Activate", this, "OnViewActivate");

    }

    public void OnViewActivate()
    {
        EmitSignal("SendPlayerData");
    }

    public void OnSendPlayerData()
    {
        PlayerEntity entity = new PlayerEntity(
            GameStats.GetId(),
            "Aidan Debug",
            Main.VERSION,
            HandCache.Get(PlayerTypes.Player).GetIds()
        );

        NetworkingManager.Instance.SendPlayerEntity(entity, this, "OnSendSuccess", "OnSendFailure");
    }

    public void OnSendSuccess()
    {
        GD.Print("Success");
        EmitSignal("GetOpponentData");
    }

    public void OnSendFailure(int responseCode, string reason)
    {
        GD.Print("Failed", responseCode, reason);
        //EmitSignal("PromptSendingRetry");
    }

    public void OnGetOpponentData()
    {
        NetworkingManager.Instance.GetPlayerEntity(this, "OnGetSuccess", "OnGetFailure");
    }

    public void OnGetSuccess(PlayerEntity entity)
    {
        HandCache.Store(new Hand(entity.Cards), PlayerTypes.Opponent);
        EmitSignal("BattleReady");
    }

    public void OnGetFailure(int responseCode, string reason)
    {
        GD.Print("Getting failed", responseCode, reason);
        //EmitSignal("PromptGettingRetry");
    }

    public void OnBattleReady()
    {
        EmitSignal("NextPhase", PhaseTypes.Battle);
    }
}
