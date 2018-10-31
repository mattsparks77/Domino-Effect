using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProgressTracker : NetworkBehaviour {

	GameObject winDisplay;
    List<EndChain> endDominos = new List<EndChain>();
	//int numberOfTargets = 0;
	int targetsHit = 0;

	// Use this for initialization
	void Start () {
		winDisplay = GameObject.FindGameObjectWithTag("Win");
		winDisplay.SetActive(false);
		//numberOfTargets = FindObjectsOfType<EndChain>().Length;
		//print("Number of targets: " + endDominos.Count);
	}

    //registers an EndChain script with the progress tracker or unregisters
    public void RegisterEnder(EndChain ender, bool register) {
        if (register && !endDominos.Contains(ender))
            endDominos.Add(ender);
        else if (!register && endDominos.Contains(ender))
            endDominos.Remove(ender);
    }
	
	public void TargetHit(){
		++targetsHit;
		CheckWin();
		print("Targets hit: " + targetsHit);
	}

	void CheckWin(){
        if (targetsHit >= endDominos.Count)
			winDisplay.SetActive(true);
	}

	[Server]
	public void Reset(){
		targetsHit = 0;
		winDisplay.SetActive(false);
		RpcReset();
	}

	[ClientRpc]
	public void RpcReset(){
		targetsHit = 0;
		winDisplay.SetActive(false);
		print("Targets hit: " + targetsHit);
	}
}
