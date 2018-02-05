using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;

public class NetworkManagers : MonoBehaviour {

    public class MyMsgType{
        public static short Event = MsgType.Highest + 1;
    };

    NetworkClient myClient;
	List<MatchInfoSnapshot> matchList = new List<MatchInfoSnapshot>();
	NetworkMatch networkMatch;

	int playerCount = 0;
	int playerReady = 0;
	uint matchSize =0;
	bool gotResult = false;

	void Awake(){
		
		networkMatch = gameObject.AddComponent<NetworkMatch>();
	}

//	void OnGUI()
//	{
//		// You would normally not join a match you created yourself but this is possible here for demonstration purposes.
//		if (GUILayout.Button("Create Room"))
//		{
//
//			//networkMatch.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
//		}
//
//		if (GUILayout.Button("List rooms"))
//		{
//			networkMatch.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
//		}
//
//		if (matchList.Count > 0)
//		{
//			GUILayout.Label("Current rooms");
//		}
//		foreach (var match in matchList)
//		{
//			if (GUILayout.Button(match.name))
//			{
//				networkMatch.JoinMatch(match.networkId, "", "", "", 0, 0, OnMatchJoined);
//			}
//		}
//	}

	public void CreateMatch(){
	
		string matchName = "room";
		matchSize = 2;
		bool matchAdvertise = true;
		string matchPassword = "";

		networkMatch.CreateMatch(matchName, matchSize, matchAdvertise, matchPassword, "", "", 0, 0, OnMatchCreate);
	}

	public void JoinGame(){
		networkMatch.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
	}
		
    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo){
        if (success){
            Debug.Log("Create match succeeded");
			NetworkServer.RegisterHandler (MyMsgType.Event, OnMSg);
			NetworkServer.Listen(matchInfo, 9000);
            //Utility.SetAccessTokenForNetwork(matchInfo.networkId, matchInfo.accessToken);
        }
    }
	public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches){
		if (success && matches != null && matches.Count > 0){
			networkMatch.JoinMatch(matches[0].networkId, "", "", "", 0, 0, OnMatchJoined);
		}
	}

	public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo){
		if (success){
			Debug.Log("Join match succeeded");

			//Utility.SetAccessTokenForNetwork(matchInfo.networkId, matchInfo.accessToken);
			myClient = new NetworkClient();
			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			myClient.RegisterHandler(MyMsgType.Event, OnEvent);
			myClient.Connect(matchInfo);
		}
	}

	public void ResetServerRound(){
	
		this.playerReady = 0; 
	}


	//server
	private void OnMSg(NetworkMessage netMsg){
		print ("server got msg");
		//NetworkServer.SendToAll (MyMsgType.Event,netMsg.ReadMessage<StringMessage>());

		StringMessage msg = netMsg.ReadMessage<StringMessage> ();

		switch (msg.value) {

		case "Connect":
			playerCount++;
			print (playerCount + " - " + matchSize);
			if (playerCount == matchSize) {
				NetworkServer.SendToAll (MyMsgType.Event, msg);
			}
			break;
		case "Ready":
			playerReady++;
			if (playerReady == matchSize) {
				NetworkServer.SendToAll (MyMsgType.Event, msg);
			}
			break;
		case "Next":
			NetworkServer.SendToAll (MyMsgType.Event, msg);
			break;
		default: NetworkServer.SendToAll (MyMsgType.Event, msg);
			break;
		}
	}
	//client
	private void OnConnected(NetworkMessage netMsg){
		Debug.Log("Connected to server");

		Manager.manager.CreateGame ();
	}

	private void OnEvent(NetworkMessage netMsg){
		StringMessage msg = netMsg.ReadMessage<StringMessage> ();
		Debug.Log("OnScoreMessage " + msg.value);
		string[] t = msg.value.Split ("+" [0]);
		if (t.Length == 1) {
			switch (msg.value) {

			case "Connect":
				Manager.manager.ChangeGameState ("WaitingForPlayer");
				break;
			case "Ready":
				this.gotResult = false;
				Manager.manager.ChangeGameState ("WaitingForReady");
				break;
			case "Next":
				Manager.manager.ChangeGameState ("WaitingNextRound");
				break;
			}
			if (!gotResult) {
				string[] s = msg.value.Split ("/" [0]);
				if (s.Length == 2) {
					Debug.Log (msg.value);
					switch (s [0]) {
					case "Result":
						this.gotResult = true;
						Manager.manager.currentGame.UpdateScore (false, int.Parse (s [1]));
						Manager.manager.ChangeGameState ("WaitingForResult");
						break;
					case "Result1":
						this.gotResult = true;
						Manager.manager.currentGame.UpdateScore (true, int.Parse (s [1]));
						Manager.manager.ChangeGameState ("WaitingForResult");
						break;
					}
				}
			}
		} else {
			if (Manager.manager.isHost && t.Length == 2) {
				Manager.manager.currentGame.SetIds (t [1]);
			} else {
				if (t.Length == 3 && !Manager.manager.isHost) {
					Manager.manager.currentGame.SetPictures (t [1]);
					Manager.manager.currentGame.SetTarget (int.Parse (t [2]));
				}
			}
		}
	}

	public void SendMessage(string s){
		var msg = new StringMessage (s);
		myClient.Send (MyMsgType.Event, msg);
	}

    void Start(){
        NetworkManager.singleton.StartMatchMaker();
    }

    //call this method to request a match to be created on the server
    public void CreateInternetMatch(string matchName)
    {
		matchSize = 2;
		NetworkManager.singleton.matchMaker.CreateMatch(matchName, matchSize, true, "", "", "", 0, 0, OnInternetMatchCreate);
    }

    //this method is called when your request for creating a match is returned
    private void OnInternetMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            Debug.Log("Create match succeeded");

            MatchInfo hostInfo = matchInfo;
            NetworkServer.Listen(hostInfo, 9000);
			NetworkServer.RegisterHandler (MyMsgType.Event, OnMSg);

			myClient = NetworkManager.singleton.StartHost(hostInfo);
			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			myClient.RegisterHandler(MyMsgType.Event, OnEvent);

			Manager.manager.isHost = true;
        }
        else
        {
            Debug.LogError("Create match failed");
        }
    }

    //call this method to find a match through the matchmaker
    public void FindInternetMatch(string matchName)
    {
        NetworkManager.singleton.matchMaker.ListMatches(0, 10, matchName, true, 0, 0, OnInternetMatchList);
    }

    //this method is called when a list of matches is returned
    private void OnInternetMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        if (success)
        {
            if (matches.Count != 0)
            {
                Debug.Log("A list of matches was returned");

                //join the last server (just in case there are two...)
                NetworkManager.singleton.matchMaker.JoinMatch(matches[matches.Count - 1].networkId, "", "", "", 0, 0, OnJoinInternetMatch);
            }
            else
            {
                Debug.Log("No matches in requested room!");
            }
        }
        else
        {
            Debug.LogError("Couldn't connect to match maker");
        }
    }

    //this method is called when your request to join a match is returned
    private void OnJoinInternetMatch(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            Debug.Log("Able to join a match");

            MatchInfo hostInfo = matchInfo;
			myClient = NetworkManager.singleton.StartClient(hostInfo);
			myClient.RegisterHandler(MsgType.Connect, OnConnected);
			myClient.RegisterHandler(MyMsgType.Event, OnEvent);
        }
        else
        {
            Debug.LogError("Join match failed");
        }
    }

	public void Disconnect(){
		myClient.Disconnect ();
	}
}
