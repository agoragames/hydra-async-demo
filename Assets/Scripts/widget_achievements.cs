using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;

//Unfinished script. This will be an simple example of how to display achievements

public class widget_achievements : MonoBehaviour {
	
	public List<Achievement> myAchievements;
	public Achievement achievements;
		
	// Use this for initialization
	public void loadMyAchievements()
	{
		Client.Instance.Achievements.All(delegate(List<Achievement> achievements, Request req){
			this.myAchievements = achievements;
		});		
	}
	
	public void displayAchievements()
	{
		if (myAchievements != null)
    	{
        	for (int i = 0; i < myAchievements.Count; i++)
        	{
				GUI.Label(new Rect(10, 10 + (60 * i), 500, 500), myAchievements[i].Image + "       " + myAchievements[i].Name
					+ "       " + myAchievements[i].Description + "       " + myAchievements[i].Points);					
			}
    	}	
	}
}
