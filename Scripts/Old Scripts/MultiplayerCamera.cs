using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Camera))]
public class MultiplayerCamera : NetworkBehaviour {

	Camera cam;

	// Use this for initialization
	void Awake () {
		if (!isLocalPlayer)
			cam.enabled = false;
	}
}
