using UnityEngine;
using System.Collections;

public class player : MonoBehaviour 
{
    public float speed = 10f;

    void FixedUpdate(){
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		
		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
		
		rigidbody.AddForce(movement * speed);
    }
}