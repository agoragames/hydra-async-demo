using UnityEngine;
using System.Collections;

public class cameracontroller : MonoBehaviour 
{	
	public GameObject player;
	private Vector3 offset;
	
	//Creates a camera object
	public void SetupCamera () 
	{
		offset = transform.position;
		player = GameObject.Find("Player(Clone)");
	}
}
