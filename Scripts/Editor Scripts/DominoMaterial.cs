using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class DominoMaterial : MonoBehaviour
{
    Material defaultMaterial;
    Material correctMaterial;
    Material overrideMaterial = null;
    MeshRenderer meshRenderer;

	// Use this for initialization
	void Start(){
        meshRenderer = GetComponent<MeshRenderer>();
        correctMaterial = defaultMaterial = meshRenderer.material;
	}

    //overrides the look of this object; set it to null to reset
    public void SetMaterialOverride(Material material) {
        overrideMaterial = material;
        if (overrideMaterial != null)
            meshRenderer.material = overrideMaterial;
        else
            meshRenderer.material = correctMaterial;
    }

    //sets what the correct look of the domino should be when the override is off
    public void SetCorrectMaterial(Material material) {
        correctMaterial = material;
        if (correctMaterial == null)
            correctMaterial = defaultMaterial;
        if (overrideMaterial == null)
            meshRenderer.material = correctMaterial;
    }
}
