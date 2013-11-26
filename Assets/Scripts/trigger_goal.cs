using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;

public class trigger_goal : MonoBehaviour {
	
	public ingamemenu ingamemenuscript;
	public GameObject gamecode;
	
	public void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.name == "Player(Clone)")
		{
			ingamemenuscript = gamecode.GetComponent<ingamemenu>();
			ingamemenuscript.leaveGame();
			ingamemenuscript.UpdateMatchScore();
		}
	}
}
