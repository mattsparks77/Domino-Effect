using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spring : MonoBehaviour {
	public GameObject ToSpring;
	private GameObject player;
	// Use this for initialization
	public float bounceForce = 10f;
	void Start()
	{
		player = GameObject.FindGameObjectWithTag ("Player");
	}
	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.tag == "Domino")
			
	
		{ ToSpring.GetComponent<Rigidbody>().AddForce(bounceForce * transform.up, ForceMode.VelocityChange); } } 
}
