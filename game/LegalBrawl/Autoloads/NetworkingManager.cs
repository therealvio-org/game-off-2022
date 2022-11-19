using Godot;
using Godot.Collections;

using System.Text;

public class NetworkingManager : Node
{
    private const string _extension = "/prod/v1/playerHand";
    public override void _Ready()
    {
        string playerId = "6d0506fc-c6bf-4dd6-a3ec-cca4515990d1";
        //CreateRequest(MakeGetURL(playerId));
    }

    public void CreateRequest(string url)
    {
        HTTPRequest request = new HTTPRequest();
        AddChild(request);
        request.Request(url, new string[] { $"x-api-key: {DebugKeys.API_KEY}" });
        request.Connect("request_completed", this, "OnRequestCompleted");
    }

    public string MakeGetURL(string playerId)
    {
        return $"{DebugKeys.BASE}{_extension}?playerId={playerId}";
    }

    public void OnRequestCompleted(int result, int response_code, string[] headers, byte[] body)
    {
        GD.Print(response_code);
        JSONParseResult json = JSON.Parse(Encoding.UTF8.GetString(body));
        Opponent opponent = new Opponent((Dictionary)json.Result);
    }

    public struct Opponent
    {
        public string PlayerId;
        public string PlayerName;
        public string Version;
        public int[] Cards;

        public Opponent(Dictionary dict)
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
}

