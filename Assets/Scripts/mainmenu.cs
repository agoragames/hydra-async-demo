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
	public Match match;
	public static string matchID;
	public List<MatchPlayer> playersList;
	
	//These variables are for the objects that will be loaded when the level starts, or upon entering a game.
	public GameObject playerPrefab;
	public GameObject worldPrefab;
	public GameObject lightingPrefab;
	public GameObject cameraPrefab;
	public GameObject Timer;
	public GameObject menuBackground;
	public GameObject ChallengeHeaderText;
	
	//Other scripts in this project that this file will reference
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
	
	//Defines if you are joining the match as the creator, or the opponent accepting the challenge.
	public bool isOpponent; 
	
	//GUI elements that will define the message of whether or not the player has won or loss the match.
	public GUIText WinLossMessage;
	public string text;
	
	//Trackes stats that are to he submitted to Hydra
	object matchesPlayed;
	object globalWins;
	object globalLosses;
	
	//The method will pull in stats from Hydra to display back to the user later. 
	//This data will not be retrieved unless Hydra is initialized.
	void getPlayerData()
	{
		if (Client.Instance.IsInitalized) {
			userName = Client.Instance.MyAccount.Identity.UserName;
			matchesPlayed = Client.Instance.MyProfile["data.matchesPlayed"];
			globalWins = Client.Instance.MyProfile["data.wins"];
			globalLosses = Client.Instance.MyProfile["data.losses"];
		}
	}
	
	//Method for the function and UI to update the username in Hydra.
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
		//Single player game (loads game components, but doesn't create an official Match or "Challenge in Hydra
		if (GUI.Button(new Rect(100, 100, 250, 100), "SINGLE PLAYER\n(Practice Offline)"))
        	loadGameComponents();
			createCamera();
		
		//Keeps the following UI disabled until Hydra has loaded [Client.Instance.Initialize]
		GUI.enabled = Client.Instance.IsInitalized;
		
		//Prompts users to update their username before allowing them to play. 
		//Default Hydra usernames are too long for our UI
		if (userName.Length > 15) {
			WinLossMessage.text = "UPDATE YOUR USERNAME TO PLAY ONLINE [15 char limit]";
		}
		
		//Calls the function to pull player data to display in the UI
		getPlayerData();
		
		GUI.Box(new Rect(400, 100, 480, 100), "");
		GUI.Box(new Rect(400, 235, 480, 350), "");
		GUI.Label(new Rect(410, 110, 200, 50), "Username: \n" + userName);
		GUI.Label(new Rect(600, 110, 200, 50), "Wins: \n" + globalWins);
		GUI.Label(new Rect(700, 110, 200, 50), "Losses: \n" + globalLosses);
		
		//Polls for an updated username so that the player can instantly play upon update
		updateUsername();
			
		//Calls the method to create a Hydra match and start a maze game
		if (GUI.Button(new Rect(100, 220, 250, 100), "CREATE A CHALLENGE\n(Beat the maze as fast as you can!)"))
			createamatch();
	
		//Calls the method to load all public matches from Hydra.
		if (Client.Instance.IsInitalized) {
			if (publicMatches == null) {
				loadPublicMatches();
			}
			displayPublicMatches();
		}
	}
	
	//This method will create a Hydra match, and load the game world and UI.
	void createamatch()
	{
		//The username has to be under 15 characters or the Hydra match will not be created.
		if (userName.Length < 15) {
			
			//Hydra method to create a new match. Don't forget that you will need to create a "Match Type" in the Hydra 
			Client.Instance.Match.CreateNew("standard-online-match", Match.Access.Public, delegate(Match match, Request req) {
				this.match = match;
				match.Load(delegate(Request request) {
					currentMatchCreatorName = match["data.matchCreatorUsername"];
				});
				currentMatchName = match.Name;
				
				//Sets the username of the match creator and stores it on the match object in Hydra.
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
		Client.Instance.Match.LoadPublic(delegate(List<Match> matches, Request req){
			this.publicMatches = matches;
		});
	}
	
	//Displays the list of available "public" matches in Hydra.
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
	
	//Joins an open match session in Hydra
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
	
	//Closes the match and marks it as complete in Hydra.
	private void CloseRoom(Match match)
	{
		match.Complete(delegate(Request req){});
		this.match = match;
		timerscript.enabled = false;
	}
	
	//Pulls the data around a specific match
	void getMatchData()
	{
		currentMatchName = match.Name;
		playersList = match.Players;
		currentMatchCreatorName = playersList[0].Identity.UserName;
		currentMatchCreatorTime = match["data.creatorTime"];
		currentMatchOpponentName = match["data.matchOpponentUsername"];
	}
	
	//Function to load all necessary game components once a match starts
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
	
	//creates the camera object for gameplay
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
