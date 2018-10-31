using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script that allows the player to knock down this domino to start.
/// 
/// I should add functionality to bind the appearance of the domino
/// to the isStartDomino state.
/// </summary>
[RequireComponent(typeof(DominoMaterial))]
public class StartChain : NetworkBehaviour {

    [SerializeField] Material startMaterial;

    DominoMaterial materialManager;
    bool isStartDomino = false;
    public bool IsStartDomino {
        get { return isStartDomino; }
        set{
            if (value) {
                EndChain thisEnd = GetComponent<EndChain>();
                if (thisEnd && thisEnd.IsEndDomino) {
                    thisEnd.IsEndDomino = false;
                }
                materialManager.SetCorrectMaterial(startMaterial);
            } else
                materialManager.SetCorrectMaterial(null);
            isStartDomino = value;
        }
    }

    //public GameObject startDomino;
    [HideInInspector] public float _x = 100;
    [HideInInspector] public float _y = 0;
    [HideInInspector] public float _z = 0;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool canReset = false;

	void Awake ()
    {
        rb = GetComponent<Rigidbody>();
        materialManager = GetComponent<DominoMaterial>();
	}

	[ClientRpc]
	public void RpcPlayerKnock(){
		if (canReset == false){
			canReset = true;
			RpcKnockDomino(_x, _y, _z);
		}
	}

    void RpcKnockDomino(float x, float y, float z)
    {
        if(isStartDomino)
		    rb.AddRelativeForce(x, y, z);
    }

	[Server]
	public void Reset(){
		canReset = false;
		RpcReset();
	}
		
	[ClientRpc]
    public void RpcReset()
    {
        canReset = false;
    }
}
