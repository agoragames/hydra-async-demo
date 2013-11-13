using UnityEngine;
using System.Collections;
using AgoraGames;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;
using System.Collections.Generic;


public class mainmenu : MonoBehaviour {
	
	//These variables are used to capture and update the Hydra Account Username. 
	//These are used to store and display the UserName locally once its recovered from Hydra.
	public string userName;
	string newUsername = "";
	
	//These variables are for the Hydra Matches that are created/joined
	List<Match> publicMatches;
	string [] publicMatchNames;
	List<string> fields;
	public Match match;
	public static string matchID;
	
	//These variables are for the prefabs that will be loaded when the level starts.
	public GameObject playerPrefab;
	public GameObject worldPrefab;
	public GameObject lightingPrefab;
	public GameObject cameraPrefab;
	public GUIText WinLossMessage;
	
	public GameObject Timer;
	public GameObject menuBackground;
	public GameObject ChallengeHeaderText;
	public mainmenu menumainscript;
	public cameracontroller camerascript;
	public ingamemenu menugamescript; 
	public timer timerscript;
	
	//These variables are used to capture data about the current match that the player is in.
	public string currentMatchName;
	public object currentMatchCreatorName;
	public object currentMatchCreatorTime;
	public object currentMatchOpponentName;
	public object currentMatchOpponentTime;
	
	public widget_achievements achievementscript;
	public bool isOpponent;      
	public string text;
	object matchesPlayed;
	object globalWins;
	object globalLosses;
	public List<MatchPlayer> playersList;

	void getPlayerData()
	{
		if (Client.Instance.IsInitalized) {
			userName = Client.Instance.MyAccount.Identity.UserName;
			matchesPlayed = Client.Instance.MyProfile["data.matchesPlayed"];
			globalWins = Client.Instance.MyProfile["data.wins"];
			globalLosses = Client.Instance.MyProfile["data.losses"];
		}
	}
	
	public void updateUsername()
	{
		newUsername = GUI.TextField(new Rect(410, 160, 200, 20), newUsername, 15);
		if (GUI.Button(new Rect(620, 160, 200, 20), "Change Your Username")) {
            Identity newIdentity = new Identity(newUsername);
			Client.Instance.MyAccount.UpdateIdentity(newIdentity, delegate(Request req)
            {
                UpdateAccount();
                newUsername = "";
            });
        }
	}
	
	void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 250, 100), "SINGLE PLAYER\n(Practice Offline)"))
        	loadGameComponents();
			createCamera();
		
		GUI.enabled = Client.Instance.IsInitalized;
		
		if (userName.Length > 15) {
			WinLossMessage.text = "UPDATE YOUR USERNAME TO PLAY ONLINE [15 char limit]";
		}
				
		getPlayerData();
		
		GUI.Box(new Rect(400, 100, 480, 100), "");
		GUI.Box(new Rect(400, 235, 480, 350), "");

		GUI.Label(new Rect(410, 110, 200, 50), "Username: \n" + userName);
		GUI.Label(new Rect(600, 110, 200, 50), "Wins: \n" + globalWins);
		GUI.Label(new Rect(700, 110, 200, 50), "Losses: \n" + globalLosses);

		updateUsername();
			
		if (GUI.Button(new Rect(100, 220, 250, 100), "CREATE A CHALLENGE\n(Beat the maze as fast as you can!)"))
			createamatch();
		
		if (GUI.Button(new Rect(790, 206, 90, 25), "Refresh List")) {
		}
		
		if (Client.Instance.IsInitalized) {
			if (publicMatches == null) {
				loadPublicMatches();
			}
			displayPublicMatches();
		}
	}

	void createamatch()
	{
		if (userName.Length < 15) {
			Client.Instance.Match.CreateNew("standard-online-match", Match.Access.Public, delegate(Match match, Request req) {
				this.match = match;
				match.Load(delegate(Request request) {
					currentMatchCreatorName = match["data.matchCreatorUsername"];
				});
				currentMatchName = match.Name;
				
				Commands commands = new Commands();
				commands.SetValue("data.matchCreatorUsername", userName);
				match.Update(commands, null);
			});
			
			menugamescript = GetComponent<ingamemenu>();
			
			loadGameComponents();
			isOpponent = false;
			menugamescript.isOpponent = false;
		}
	}
	
	public void loadPublicMatches()
	{
		//Client.Instance.Match.LoadPublic(delegate(List<Match> matches, Request req){
		//	this.publicMatches = matches;
		//});
		
		Client.Instance.Match.LoadPublic(delegate(List<Match> matches, Request req){
			this.publicMatches = matches;
		});
	}
	
	void displayPublicMatches()
	{
		if (userName.Length > 15) {
			// do something
		}
		else 
		{
			if (publicMatches != null)
	    	{
	        	for (int i = 0; i < publicMatches.Count; i++)
	        	{
					playersList = publicMatches[i].Players;
					currentMatchCreatorName = playersList[0].Identity.UserName;
					
					GUI.Label(new Rect(410, 250 + (40 * i), 250, 100), currentMatchCreatorName + " created a challenge.");
					
					if (GUI.Button(new Rect(670, 250 + (40 * i), 200, 30), "Accept This Challenge!")) {
						JoinRoom(publicMatches[i]);	
						getMatchData();
						loadGameComponents();
					}
				
					if (i == 7)
					{
						break;
					}
				}
	    	}
		}
	}
	
	private void JoinRoom(Match match)
	{
		menugamescript = GetComponent<ingamemenu>();
		
		match.Join(delegate(Request req){});
		match.JoinSession();
		this.match = match;
		matchID = match.Id;
		isOpponent = true;
		menugamescript.isOpponent = true;
		Commands commands = new Commands();
		commands.SetValue("data.matchOpponentUsername", userName);
		match.Update(commands, null);
	}
	
	private void LeaveRoom(Match match)
	{
		this.match = match;
		timerscript.enabled = false;
	}
	
	private void CloseRoom(Match match)
	{
		match.Complete(delegate(Request req){});
		this.match = match;
		timerscript.enabled = false;
	}
	
	void getMatchData()
	{
		currentMatchName = match.Name;
		playersList = match.Players;
		currentMatchCreatorName = playersList[0].Identity.UserName;
		currentMatchCreatorTime = match["data.creatorTime"];
		currentMatchOpponentName = match["data.matchOpponentUsername"];
		//currentMatchPlayer2Name = playersList[1].Identity.UserName;
		//currentMatchPlayer2Score = match["data.player2score"];
	}
	
	void loadGameComponents()
	{
		menumainscript = GetComponent<mainmenu>();
		menugamescript = GetComponent<ingamemenu>();
		menumainscript.enabled = false;
		menugamescript.enabled = true;
		Instantiate (playerPrefab);
		Instantiate (worldPrefab);
		Instantiate (lightingPrefab);
		Instantiate (cameraPrefab);		
		timerscript = Timer.GetComponent<timer>();
		timerscript.enabled = true;
		timerscript.timeLeft = 0.0f;
		Timer.active = true;
		WinLossMessage.text = "";
		menuBackground.SetActive(false);
		ChallengeHeaderText.SetActive(false);	
	}
	
	void createCamera()
	{
		camerascript = cameraPrefab.GetComponent<cameracontroller>();
		camerascript.SetupCamera();
	}	
	
	protected void UpdateAccount()
    {
        text = MiniJSON.Json.Serialize(Client.Instance.MyAccount.Data, true);
    }
}
