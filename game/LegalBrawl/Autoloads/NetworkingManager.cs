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
        string url = Endpoints.PlayerHand();
        string[] headers = new string[] { $"x-api-key: {DebugKeys.API_KEY}", "Content-Type: application/json" };
        Dictionary handInfo = new Dictionary();
        handInfo.Add("handInfo", entity.ToDict());
        string body = JSON.Print(handInfo);

        request.Request(url, headers, true, HTTPClient.Method.Put, body);
        request.Connect("request_completed", this, "OnPutPlayerEntityComplete");

        // Post if that fails
        if (IsConnected("AttemptPostEntity", this, "OnAttemptPostEntity"))
            Disconnect("AttemptPostEntity", this, "OnAttemptPostEntity"); // Have to disconnect so I can bind a new entity
        Connect("AttemptPostEntity", this, "OnAttemptPostEntity", new Array() { entity }, (uint)ConnectFlags.Oneshot);

        // Callback when complete
        Connect("SentPlayerEntitySuccess", caller, successSignal, null, (uint)ConnectFlags.Oneshot);
        Connect("RequestFailed", caller, failureSignal, null, (uint)ConnectFlags.Oneshot);
    }

    public void OnPutPlayerEntityComplete(int result, int response_code, string[] headers, byte[] body)
    {
        string message = Encoding.UTF8.GetString(body);
        if (response_code == 200)
        {
            EmitSignal("SentPlayerEntitySuccess");
            return;
        }
        switch (response_code)
        {
            case 404:
                // GD.Print("404: ", message, " attempting post");
                EmitSignal("AttemptPostEntity");
                break;

            case 403:
                // GD.Print("403: ", message);
                EmitSignal("SentPlayerEntitySuccess");
                break;

            default:
                EmitSignal("RequestFailed", response_code, message);
                break;
        }
    }

    public void OnAttemptPostEntity(PlayerEntity entity)
    {
        HTTPRequest request = CreateRequest();
        string url = Endpoints.PlayerHand();
        string[] headers = new string[] { $"x-api-key: {DebugKeys.API_KEY}", "Content-Type: application/json" };
        Dictionary handInfo = new Dictionary();
        handInfo.Add("handInfo", entity.ToDict());
        string body = JSON.Print(handInfo);
        request.Request(url, headers, true, HTTPClient.Method.Post, body);
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
        string query = Endpoints.PlayerIdQuery(GameStats.GetId());
        string url = Endpoints.PlayerHand(query);
        request.Request(url, new string[] { $"x-api-key: {DebugKeys.API_KEY}" });
        request.Connect("request_completed", this, "OnGetPlayerEntityComplete");

        // Respond with value
        Connect("PlayerEntityAvailable", caller, successSignal, null, (uint)ConnectFlags.Oneshot);
        Connect("RequestFailed", caller, failureSignal, null, (uint)ConnectFlags.Oneshot);
    }

    public void OnGetPlayerEntityComplete(int result, int response_code, string[] headers, byte[] body)
    {
        string message = Encoding.UTF8.GetString(body);

        if (response_code == 200)
        {
            JSONParseResult json = JSON.Parse(message);
            PlayerEntity playerEntity = new PlayerEntity((Dictionary)json.Result);
            EmitSignal("PlayerEntityAvailable", playerEntity);
            return;
        }

        EmitSignal("RequestFailed", response_code, message);
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