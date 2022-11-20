using Godot;
using Godot.Collections;

using System.Text;

public class NetworkingManager : Node
{
    public static NetworkingManager Instance { get; private set; }
    [Signal] public delegate void AttemptPostEntity(PlayerEntity entity);
    [Signal] public delegate void SentPlayerEntitySuccess();
    [Signal] public delegate void PlayerEntityAvailable(PlayerEntity entity);
    [Signal] public delegate void RequestFailed(int responseCode, string reason);

    public override void _Ready()
    {
        Instance = this;
    }

    public void SendPlayerEntity(PlayerEntity entity, Object caller, string successSignal, string failureSignal)
    {
        // Attempt to put
        HTTPRequest request = CreateRequest();
        string url = Endpoints.PlayerHand(GameStats.GetId());
        string[] headers = new string[] { $"x-api-key: {DebugKeys.API_KEY}" };
        request.Request(url, headers, true, HTTPClient.Method.Put);
        request.Connect("request_completed", this, "OnPutPlayerEntityComplete");

        // Post if that fails
        Connect("AttemptPostEntity", this, "OnAttemptPostEntity", new Array() { entity }, (uint)ConnectFlags.Oneshot);

        // Callback when complete
        caller.Connect("SentPlayerEntitySuccess", caller, successSignal, null, (uint)ConnectFlags.Oneshot);
        caller.Connect("RequestFailed", caller, failureSignal, null, (uint)ConnectFlags.Oneshot);
    }

    public void OnPutPlayerEntityComplete(int result, int response_code, string[] headers, byte[] body)
    {
        if (response_code == 200)
        {
            EmitSignal("SentPlayerEntitySuccess");
            return;
        }
        switch (response_code)
        {
            case 404:
                GD.Print("Put failed, attempting post");
                EmitSignal("AttemptPostEntity");
                break;

            case 403:
                GD.Print("Duplicate hand already exists, ignoring");
                EmitSignal("SentPlayerEntitySuccess");
                break;

            default:
                EmitSignal("RequestFailed", response_code, Encoding.UTF8.GetString(body));
                break;
        }
    }

    public void OnAttemptPostEntity(PlayerEntity entity)
    {
        HTTPRequest request = CreateRequest();
        string url = Endpoints.PlayerHand(GameStats.GetId());
        string[] headers = new string[] { $"x-api-key: {DebugKeys.API_KEY}" };
        request.Request(url, headers, true, HTTPClient.Method.Post);
        request.Connect("request_completed", this, "OnPostPlayerEntityComplete");
    }

    public void OnPostPlayerEntityComplete(int result, int response_code, string[] headers, byte[] body)
    {
        if (response_code == 200)
        {
            EmitSignal("SentPlayerEntitySuccess");
            return;
        }

        EmitSignal("RequestFailed", response_code, Encoding.UTF8.GetString(body));
    }

    public void GetPlayerEntity(Object caller, string successSignal, string failureSignal)
    {
        // @TODO Check if secret cache has api key

        // Attempt to get
        // request.Request(url, new string[] { $"x-api-key: {DebugKeys.API_KEY}" });
        HTTPRequest request = CreateRequest();
        request.Request(Endpoints.PlayerHand(GameStats.GetId()), new string[] { $"x-api-key: {DebugKeys.API_KEY}" });
        request.Connect("request_completed", this, "OnGetPlayerEntityComplete");

        // Respond with value
        caller.Connect("PlayerEntityAvailable", caller, successSignal, null, (uint)ConnectFlags.Oneshot);
        caller.Connect("RequestFailed", caller, failureSignal, null, (uint)ConnectFlags.Oneshot);
    }

    public void OnGetPlayerEntityComplete(int result, int response_code, string[] headers, byte[] body)
    {
        if (response_code == 200)
        {
            JSONParseResult json = JSON.Parse(Encoding.UTF8.GetString(body));
            PlayerEntity playerEntity = new PlayerEntity((Dictionary)json.Result);
            EmitSignal("PlayerEntityAvailable", playerEntity);
            return;
        }

        GD.Print(result);
        EmitSignal("RequestFailed", response_code, Encoding.UTF8.GetString(body));
    }

    public HTTPRequest CreateRequest()
    {
        HTTPRequest request = new HTTPRequest();
        AddChild(request);
        //request.Connect("request_completed", this, "DisposeRequest", new Array() { request });
        return request;
    }

    public void DisposeRequest(int result, int response_code, string[] headers, byte[] body, HTTPRequest request)
    {
        request.QueueFree();
    }
}