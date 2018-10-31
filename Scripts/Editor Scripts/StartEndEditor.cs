using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// NOT FINISHED
/// </summary>
public class StartEndEditor : NetworkBehaviour, IEditorMode {

    [SerializeField] float maxDistance = 100f;
    [SerializeField] KeyCode activationButton = KeyCode.Mouse0;
    [SerializeField] Material indicatorStart;
    [SerializeField] Material indicatorEnd;
    [SerializeField] Material indicatorNone;
    [SerializeField] LayerMask castObstructions;
    [SerializeField] LayerMask dominoLayer;
    [SerializeField] string modeName;

    Camera cameraObject = null;
    Domino selectedDomino = null;
    DominoMaterial materialOverride = null;
    StartChain selectedStart = null;
    EndChain selectedEnd = null;
    bool activeMode = false;

    //=============================================================================================
    // control
    //=============================================================================================

    // Use this for initialization
    void Start() {
        cameraObject = GetComponent<Camera>();
        if (cameraObject == null)
            cameraObject = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update() {
        if (!isLocalPlayer || !activeMode)
            return;
        DominoTypeEditBehavior(Input.GetKeyDown(activationButton));
    }

    void OnDisable() {
        AbandonDomino();
    }

    void OnDestroy() {
        AbandonDomino();
    }

    public void ActivateMode(bool isActive) {   //IEditorMode
        if (!isLocalPlayer)
            return;
        activeMode = isActive;
        if (!activeMode)
            AbandonDomino();
    }

    public void RotateSubMode() {}  //IEditorMode

    //=============================================================================================
    // face switching logic
    //=============================================================================================

    void DominoTypeEditBehavior(bool activate) {
        RaycastHit hit;
        Vector3 miss;
        if (!CommonFunctions.CastFromCamera(out hit, out miss, cameraObject.transform, maxDistance, castObstructions))
            AbandonDomino();  //missed, so abandon the last face

        else if (((1 << hit.collider.gameObject.layer) & dominoLayer.value) != 0) {
            //if it is a domino, we should select it
            AdoptDomino(hit.collider.GetComponent<Domino>());
            if (activate)
                ToggleDominoMode();
        } else
            AbandonDomino();
    }

    //switches the face's placement mode and updates the indicator material
    void ToggleDominoMode() {
        //cycling backwards up this if-else block (default->start->end->repeat)
        if (selectedEnd.IsEndDomino) {
            selectedEnd.IsEndDomino = false;    //become neither if end
        } else if (selectedStart.IsStartDomino) {
            selectedStart.IsStartDomino = false;    //become end domino if start
            selectedEnd.IsEndDomino = true;
        } else {
            selectedStart.IsStartDomino = true; //become start domino if neither
        }
        SetCorrectIndicatorColor();
    }


    //=============================================================================================
    // face indicator functions
    //=============================================================================================

    //sets the face's material back to what it was before it was selected
    void RestoreAppearance() {
        if (selectedDomino != null)
            materialOverride.SetMaterialOverride(null);
    }

    //stops keeping track of the targetToDelete and restores its appearance
    void AbandonDomino() {
        RestoreAppearance();
        selectedDomino = null;
    }

    //chooses a new domino to be the delete target and changes its material to the noPlace material
    void AdoptDomino(Domino newTarget) {
        if (selectedDomino == newTarget)
            return;
        AbandonDomino();
        if (newTarget == null)
            return;
        selectedDomino = newTarget;

        materialOverride = selectedDomino.GetComponent<DominoMaterial>();
        selectedEnd = selectedDomino.GetComponent<EndChain>();
        selectedStart = selectedDomino.GetComponent<StartChain>();

        //change its appearance
        SetCorrectIndicatorColor();
    }

    //sets the material of the selected face depending on whether that face can have dominos placed on it
    void SetCorrectIndicatorColor() {
        if (selectedEnd.IsEndDomino) {
            materialOverride.SetMaterialOverride(indicatorEnd);
        } else if (selectedStart.IsStartDomino) {
            materialOverride.SetMaterialOverride(indicatorStart);
        } else {
            materialOverride.SetMaterialOverride(indicatorNone);
        }
    }

    //=============================================================================================
    // UI methods
    //=============================================================================================

    public string ModeName() {
        return modeName;
    }
}
