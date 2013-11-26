using UnityEngine;
using System;
using System.Collections;
using AgoraGames;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;
using System.Collections.Generic;


public class ingamemenu : MonoBehaviour {
	
	public mainmenu menumainscript;
	public ingamemenu menugamescript;
	public string matchName;
	public object creatorName;
	public object creatorScore;
	public object opponentName;
	public object opponentScore;
	public object matchscore;
	public double myFinishTime;
	public string myUserName;
	
	public GameObject Timer;
	public GameObject ChallengeHeaderText;
	public GameObject menuBackground;
	public timer timerscript;
	public bool isOpponent;
	
	public GUIStyle customGUIStyle;
	public GUISkin customButtonStyle;
	public GUIText WinLossMessage;

	
	public Match match;
	
	//This method pull information regarding the current challenge from the Hydra match system.
	public void GetMatchData()
	{
		menumainscript = GetComponent<mainmenu>();
		match = menumainscript.match;

		matchName = menumainscript.currentMatchName;
		creatorName = match["data.matchCreatorUsername"];
		opponentName = match["data.matchOpponentUsername"];
		creatorScore = match["data.creatorTime"];
		opponentScore = match["data.opponentTime"];
		myUserName = Client.Instance.MyAccount.Identity.UserName;			
	}
	
	//This method will send an updated score to Hydra, and will also mark the match as complete.
	public void UpdateMatchScore()
	{
		timerscript = Timer.GetComponent<timer>();
		myFinishTime = timerscript.timeLeft;	
		menumainscript = GetComponent<mainmenu>();
		
		if (isOpponent == true) {
			Commands commands = new Commands();
			commands.SetValue("data.opponentTime", myFinishTime);
			menumainscript.match.Update(commands, null);
			menumainscript.match.Complete(delegate(Request request){});
			double creatorScoreDouble = (double)creatorScore;
			
			if (myFinishTime < creatorScoreDouble) 
			{
				Commands commands2 = new Commands();
				commands2.IncValue("data.wins", 1);
				Client.Instance.MyProfile.Update(commands2, null);	
				WinLossMessage.text = "You Won The Challenge!";
			}
			else 
			{
				Commands commands2 = new Commands();
				commands2.IncValue("data.losses", 1);
				Client.Instance.MyProfile.Update(commands2, null);	
				WinLossMessage.text = "You Lost The Challenge :-(";
			}
		}
		else {
			Commands commands = new Commands();
			commands.SetValue("data.creatorTime", myFinishTime);
			menumainscript.match.Update(commands, null);	
		}
		
	}
	
	void OnGUI()
	{		
		GetMatchData();
		
		if (GUI.Button(new Rect(750, 540, 200, 50), "Forfeit")) {
			leaveGame();
			if (Client.Instance.IsInitalized) {
				match.Complete(delegate(Request request){});
			}
		}		
		if (Client.Instance.IsInitalized) {

			if (isOpponent == true) {
				GUI.Label(new Rect(10, 2, 500, 100), creatorName + "'s Time: " + creatorScore + " seconds", customGUIStyle);
			}
		}
	}
	
	//This method will destroy all created game objects, and return the player to the main menu.
	public void leaveGame()
	{
		menumainscript = GetComponent<mainmenu>();
		menugamescript = GetComponent<ingamemenu>();
		
		menumainscript.enabled = true;
		menugamescript.enabled = false;
		
		Destroy(GameObject.Find("Player(Clone)"));
		Destroy(GameObject.Find("Maze(Clone)"));
		Destroy(GameObject.Find("LevelLight(Clone)"));
		Destroy(GameObject.Find("Main Camera(Clone)"));
		menuBackground.SetActive(true);
		Timer.SetActive(false);
		ChallengeHeaderText.SetActive(true);
		menumainscript.loadPublicMatches();
	}
}
