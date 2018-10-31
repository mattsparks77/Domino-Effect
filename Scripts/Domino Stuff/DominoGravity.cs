using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DominoGravity : NetworkBehaviour {

	[SyncVar]
	[SerializeField] Vector3 gravityOrientation = Vector3.zero;
	[SerializeField] bool canDelete = true;

	Rigidbody body;

	// Use this for initialization
	void Start () {
		body = GetComponentInChildren<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (gravityOrientation == Vector3.zero)
			return;
		body.AddForce(gravityOrientation.normalized * Physics.gravity.magnitude, ForceMode.Acceleration);
	}

	[Server]
	public void ServerSetGravity(Vector3 gravity){
		gravityOrientation = gravity.normalized;
	}

	[ClientRpc]
	public void RpcSetGravity(Vector3 gravity){
		gravityOrientation = gravity.normalized;
	}
		
	public bool CanBeDeleted(){
		return canDelete;
	}

	public Vector3 Gravity{
		set{ gravityOrientation = value.normalized; }
		get{ return gravityOrientation; }
	}
}
