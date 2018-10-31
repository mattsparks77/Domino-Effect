using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ResetButton : NetworkBehaviour {

	DominoTracker tracker;

	void Start(){
		tracker = FindObjectOfType<DominoTracker>();
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.R)){
			CmdReset();
		}
	}

	[Command]
	public void CmdReset(){
		tracker.CmdReset();
	}
}
