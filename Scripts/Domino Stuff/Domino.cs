using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Domino : NetworkBehaviour{
	Rigidbody dominoBody;
	Quaternion spawnRotation;
	Vector3 spawnPoint;

	//player calls reset - call reset on every player

	//on player join, existing dominoes need to subscribe for that player

	void Awake(){
		spawnPoint = transform.position;
		spawnRotation = transform.rotation;
		dominoBody = GetComponent<Rigidbody>();
	}

	[Server]
	public void Reset(){
		RpcReset();
		dominoBody.velocity = Vector3.zero;
		dominoBody.angularVelocity = Vector3.zero;
		transform.position = spawnPoint;
		transform.rotation = spawnRotation;
	}

	[ClientRpc]
	public void RpcReset(){
		//print("RPC Reset");
		dominoBody.velocity = Vector3.zero;
		dominoBody.angularVelocity = Vector3.zero;
		transform.position = spawnPoint;
		transform.rotation = spawnRotation;
	}
}
