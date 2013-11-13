using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;

public class timer : MonoBehaviour {

	public double timeLeft = 0.0f;
	
    public void Update()
    {
        timeLeft += Time.deltaTime;
		Math.Round(timeLeft, 2);
        guiText.text = "Your Time: " + timeLeft + " secs";
    }
}
