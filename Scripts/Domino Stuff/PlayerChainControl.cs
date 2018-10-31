using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerChainControl : NetworkBehaviour {

	SingletonSupport supporter;

	// Use this for initialization
	void Start () {
		supporter = FindObjectOfType<SingletonSupport>();	//will need to use this reference to disable all players' domino placement control
															//while the dominos are falling
	}
	
	// Update is called once per frame
	void Update () {
		if (isClient && isLocalPlayer){
			if (Input.GetKeyDown(KeyCode.R)){
				CmdReset();
			} else if (Input.GetKeyDown(KeyCode.E)){
				CmdKnock();
			}
		}
	}

	[Command]
	public void CmdReset(){
		FindObjectOfType<DominoTracker>().CmdReset();
	}

	[Command]
	public void CmdKnock(){
		foreach(StartChain sc in FindObjectsOfType<StartChain>())
			sc.RpcPlayerKnock();
	}
}
