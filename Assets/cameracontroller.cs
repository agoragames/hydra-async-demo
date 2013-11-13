using UnityEngine;
using System.Collections;

public class cameracontroller : MonoBehaviour 
{	
	public GameObject player;
	private Vector3 offset;
	
	// Use this for initialization
		
	public void SetupCamera () 
	{
		offset = transform.position;
		player = GameObject.Find("Player(Clone)");
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		//transform.position = player.transform.position + offset;
	}
}
