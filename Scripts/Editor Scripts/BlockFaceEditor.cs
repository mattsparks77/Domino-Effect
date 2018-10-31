using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BlockFaceEditor : NetworkBehaviour, IEditorMode {

    [SerializeField] float maxDistance;
    [SerializeField] KeyCode activationButton = KeyCode.Mouse0;
    [SerializeField] Material indicatorNoPlace;
    [SerializeField] Material indicatorPlace;
    [SerializeField] LayerMask castObstructions;
    [SerializeField] LayerMask faceLayers;
    [SerializeField] string modeName;

    Camera cameraObject = null;
    BlockFace selectedFace = null;
    bool activeMode = false;


    //=============================================================================================
    // control
    //=============================================================================================

	// Use this for initialization
	void Start () {
        cameraObject = GetComponent<Camera>();
        if (cameraObject == null)
            cameraObject = GetComponentInChildren<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer || !activeMode)
            return;
        FaceBehavior(Input.GetKeyDown(activationButton));
	}

    void OnDisable() {
        AbandonFace();
    }

    void OnDestroy() {
        AbandonFace();
    }

    public void ActivateMode(bool isActive) {   //IEditorMode
        activeMode = isActive;
        if (!activeMode)
            AbandonFace();
    }

    public void RotateSubMode() { }  //IEditorMode

    //=============================================================================================
    // face switching logic
    //=============================================================================================

    void FaceBehavior(bool activate) {
        RaycastHit hit;
        Vector3 miss;
        if (!CommonFunctions.CastFromCamera(out hit, out miss, cameraObject.transform, maxDistance, castObstructions))
            AbandonFace();  //missed, so abandon the last face

        else if (((1 << hit.collider.gameObject.layer) & faceLayers.value) != 0) {
            //if it is a block face, we should select it
            AdoptFace(hit.collider.GetComponent<BlockFace>());
            if (activate)
                ToggleFacePlacement();
        }

        else
            AbandonFace();
    }

    //switches the face's placement mode and updates the indicator material
    void ToggleFacePlacement() {
        selectedFace.Placeable = !selectedFace.Placeable;
        SetCorrectIndicatorColor();
    }


    //=============================================================================================
    // face indicator functions
    //=============================================================================================

    //sets the face's material back to what it was before it was selected
    void RestoreAppearance() {
        if (selectedFace != null)
            selectedFace.SetMaterialOverride(null);
    }

    //stops keeping track of the targetToDelete and restores its appearance
    void AbandonFace() {
        RestoreAppearance();
        selectedFace = null;
    }

    //chooses a new domino to be the delete target and changes its material to the noPlace material
    void AdoptFace(BlockFace newTarget) {
        if (selectedFace == newTarget)
            return;
        AbandonFace();
        if (newTarget == null)
            return;
        selectedFace = newTarget;
        //change its appearance
        SetCorrectIndicatorColor();
    }

    //sets the material of the selected face depending on whether that face can have dominos placed on it
    void SetCorrectIndicatorColor() {
        if (selectedFace.Placeable) {
            selectedFace.SetMaterialOverride(indicatorPlace);
        } else {
            selectedFace.SetMaterialOverride(indicatorNoPlace);
        }
    }

    //=============================================================================================
    // UI methods
    //=============================================================================================

    public string ModeName() {
        return modeName;
    }
}
