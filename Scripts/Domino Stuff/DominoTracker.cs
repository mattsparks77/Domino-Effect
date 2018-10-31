using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DominoTracker : NetworkBehaviour{

	//public void 

	[Server]
	public void CmdReset(){
		foreach (Domino d in FindObjectsOfType<Domino>()){
			d.Reset();
		}
		foreach (StartChain sc in FindObjectsOfType<StartChain>()){
			sc.Reset();
		}
		FindObjectOfType<ProgressTracker>().Reset();
	}

	[ClientRpc]
	public void RpcDebugOut(){
		print("Debugging");
	}
}
