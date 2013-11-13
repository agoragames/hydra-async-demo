using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;

public class widget_achievements : MonoBehaviour {
	
	public List<Achievement> myAchievements;
	public Achievement achievements;
	//public string url2 = "https://s3.amazonaws.com/tesla-ugc-production/527c0d0e44d90548fe2161ff:0cf1cb034ff7e4bca698d68d1a429b8c.jpg";
	//public Texture2D icon = new Texture2D(16,16, TextureFormat.DXT1, false);
	//public string www = WWW(url);
    
//	IEnumerator Start() {
		//icon = new Texture2D(16,16, TextureFormat.DXT1, false);
		//yield www;
		//www.LoadImageIntoTexture(icon);
//    }
		
	// Use this for initialization
	public void loadMyAchievements()
	{
		Client.Instance.Achievements.All(delegate(List<Achievement> achievements, Request req){
			this.myAchievements = achievements;
		});
		
	}
	
	public void displayAchievements()
	{
//		GUI.DrawTexture(Rect(20,80,256,256), icon);

		if (myAchievements != null)
    	{
        	for (int i = 0; i < myAchievements.Count; i++)
        	{
				GUI.Label(new Rect(400, 250 + (60 * i), 500, 500), myAchievements[i].Image + "       " + myAchievements[i].Name
					+ "       " + myAchievements[i].Description + "       " + myAchievements[i].Points);
				
				//if (GUI.Button(new Rect(650, 250 + (40 * i), 220, 30), "Join this Game")) 
					//JoinRoom(publicMatches[i]);						
			}
    	}	
	}
		
	// Update is called once per frame
	void Update () {
		
	}
}
