using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A face of a map block; used for the editor for now, but may have uses in the game scene too
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class BlockFace : MonoBehaviour {

	[SerializeField] Material placeableMaterial;
	[SerializeField] int placeableLayer;
	[SerializeField] Material unplaceableMaterial;
	[SerializeField] int unplaceableLayer;

	MeshRenderer meshRenderer;
	Material correctMaterial;
	Material overrideMaterial = null;	//for the editor
    EnvironmentBlock block;
    List<GameObject> attachedDominos = new List<GameObject>();

	bool placeable = true;
	public bool Placeable
	{
		get{
			return placeable;
		}
		set{
			placeable = value;
			PlaceableUpdate();
		}
	}

	// Use this for initialization
	void Start () {
		meshRenderer = GetComponent<MeshRenderer>();
        block = GetComponentInParent<EnvironmentBlock>();
		PlaceableUpdate();
	}

    void OnDestroy() {
        //destroy all attached dominos at the server if this block is destroyed
        RemoveAllDominos();
    }

	void PlaceableUpdate(){
		if (placeable){
			correctMaterial = placeableMaterial;
			gameObject.layer = placeableLayer;
		} else{
			correctMaterial = unplaceableMaterial;
			gameObject.layer = unplaceableLayer;
            RemoveAllDominos();
		}
		if (overrideMaterial == null)
			meshRenderer.material = correctMaterial;
	}

	//overrides the look of this object; set it to null to reset
	public void SetMaterialOverride(Material material){
		overrideMaterial = material;
		if (overrideMaterial != null)
			meshRenderer.material = overrideMaterial;
		else
			meshRenderer.material = correctMaterial;
	}

    //=============================================================================================
    // attached domino functions
    //=============================================================================================

    public void AddDomino(GameObject domino) {
        attachedDominos.Add(domino);
    }

    public void RemoveDomino(GameObject domino) {
        attachedDominos.Remove(domino);
    }

    void RemoveAllDominos() {
        if (block == null)
            return;
        GameObject[] dominosArray = attachedDominos.ToArray();
        foreach (GameObject o in dominosArray) {
            block.ServerDestroyAttachedDomino(o);
        }
        attachedDominos.Clear();
    }
}
