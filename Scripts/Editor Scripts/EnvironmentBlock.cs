using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script for the domino map editor. This will not be necessary in play mode
/// </summary>
public class EnvironmentBlock : NetworkBehaviour {

	Dictionary<Vector3, BlockFace> quadsDict = new Dictionary<Vector3, BlockFace>();

	// Use this for initialization
	void Start () {
		foreach (BlockFace bf in transform.GetComponentsInChildren<BlockFace>()){
			quadsDict.Add(bf.transform.localPosition.normalized, bf);
		}
	}

	//this is local - used when the player mouses over a block with delete mode on
	public void EditorChangeMaterial(Material editorOverride){
		foreach (KeyValuePair<Vector3, BlockFace> kv in quadsDict)
			kv.Value.SetMaterialOverride(editorOverride);
	}

    //destroys the domino, if this is on the server
    public void ServerDestroyAttachedDomino(GameObject domino) {
        if(isServer)
            NetworkServer.Destroy(domino);
    }
}
