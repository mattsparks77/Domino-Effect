using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DominoSpawnerTwo : NetworkBehaviour, IEditorMode {

	[SerializeField] Camera cameraObject;
	//[SerializeField] Vector3 gravityDirection = Vector3.down;		//the gravity thing will be dealt with later
	[SerializeField] Material canPlaceMaterial;
	[SerializeField] Material noPlaceMaterial;
	[SerializeField] DominoGravity dominoPrefab;
    [SerializeField] GameObject placementIndicatorPrefab;
	[SerializeField] LayerMask raycastTargets = ~0;	//must be a superset of both spawnTargets and dominoTargets; default to everything
	[SerializeField] LayerMask spawnTargets;
	[SerializeField] LayerMask dominoTargets;
	[SerializeField] float rotationSensitivity = 2f;
	[SerializeField] float placeDistance = 10f;
    [SerializeField] string modeName;

	GameObject placementIndicator;
	MeshRenderer indicatorRenderer;
	//SingletonSupport supporter;
	float dominoRotation = 0f;

	GameObject targetToDelete = null;
	Material targetOldMaterial;	//note: give the domino script a reference to the meshrenderer component to make this easier and faster

    bool activeMode = false;

    //=============================================================================================
	// control
    //=============================================================================================

	void Start(){
        if (!isLocalPlayer)
            return;
        print("Start");
		SpawnIndicator();
	}

    void Update() {
        if (!isLocalPlayer || !activeMode)
            return;
        //run functionality
        float scroll = Input.mouseScrollDelta.y;
        bool activate = Input.GetKeyDown(KeyCode.Mouse0);
        RotateDomino(scroll);
        DominoAddRemoveLogic(activate);
    }

    void OnEnable(){
        Activate();
	}

	void OnDisable(){
        Deactivate();
	}

    void OnDestroy(){
        if (!isLocalPlayer)
            return;
        //CmdDestroy(placementIndicator);
    }

    void Activate() {
        if (!isLocalPlayer)
            return;
        if (placementIndicator != null)
            CmdSetActive(placementIndicator, true);
            //placementIndicator.SetActive(true);
        //else
            //SpawnIndicator();
    }

    void Deactivate() {
        if (!isLocalPlayer)
            return;
        //need to switch the appearance of the delete-highlighted domino back to normal
        DelAbandonDomino();
        //and make the indicator disappear
        if (placementIndicator != null)
            CmdSetActive(placementIndicator, false);
            //placementIndicator.SetActive(false);
    }

    public void ActivateMode(bool isActive) {   //IEditorMode
        if (!isLocalPlayer)
            return;
        activeMode = isActive;
        if (!activeMode)
            Deactivate();
        else
            Activate();
    }

    public void RotateSubMode() { }  //IEditorMode

    //=============================================================================================
    // initialization functions
    //=============================================================================================

    void SpawnIndicator(){
        //placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //placementIndicator.transform.localScale = dominoPrefab.transform.localScale;
        //placementIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");
        //then spawn for server
        print("Trying to spawn");
		CmdSpawnIndicator();
	}

    //=============================================================================================
	// domino system behavior
    //=============================================================================================

    void RotateDomino(float input) {
        dominoRotation += input * rotationSensitivity;
        dominoRotation = dominoRotation % 360;
    }

	void DominoAddRemoveLogic(bool activate){
		//do a raycast
		RaycastHit hit;
		Vector3 miss;
        if (!CastFromCamera(out hit, out miss)) {
            //if we didn't find a hit, just hover the indicator there
            HoverLogic(miss);
        }

        //check if what we hit is a domino
        else if (((1 << hit.collider.gameObject.layer) & dominoTargets.value) != 0) {
            //if it is a domino, then we should be in delete mode
            DeletionLogic(hit, activate);
        }

        //check if what we hit is a spawnable surface
        else if (((1 << hit.collider.gameObject.layer) & spawnTargets.value) != 0) {
            //if it is, then we should be in placement mode
            PlacementLogic(hit, activate);
        }

        else
            HoverLogic(hit.point);
	}

	//we did not hit anything at all
	void HoverLogic(Vector3 miss){
		Vector3 hoverPoint = miss;
		Quaternion placementRotation = CreateDominoRotation(transform.up, dominoRotation);

		DelAbandonDomino();
		AdjustIndicatorVisibility(true);
		AdjustIndicatorPosition(hoverPoint);
		AdjustIndicatorRotation(placementRotation);
		AdjustIndicatorColor(false);
	}

	//at this point we know that we did not hit a domino
	void PlacementLogic(RaycastHit hit, bool activate){
		Vector3 placementPoint = SmartPlacePoint(hit);
		Quaternion placementRotation = CreateDominoRotation(hit.normal, dominoRotation);
		bool canPlace = CommonFunctions.CanPlace(placementPoint, placementIndicator.transform.localScale, placementRotation, raycastTargets);

		DelAbandonDomino();
		AdjustIndicatorVisibility(true);
		AdjustIndicatorPosition(placementPoint);
		AdjustIndicatorRotation(placementRotation);
		AdjustIndicatorColor(canPlace);

		if (canPlace && activate){
            //then spawn the domino
            CmdSpawnDomino(placementPoint, placementRotation, -hit.normal, hit.collider.gameObject);
		}
	}

	//at this point we know that we hit a domino
	void DeletionLogic(RaycastHit hit, bool activate){
		AdjustIndicatorVisibility(false);

		DelAdoptDomino(hit.collider.gameObject);

		if (targetToDelete != null && activate){
            //then delete the domino
            CmdDestroy(hit.collider.gameObject);
		}
	}

    //=============================================================================================
	// helper functions
    //=============================================================================================

	//does a raycast from the camera against raycast targets
	bool CastFromCamera(out RaycastHit hit, out Vector3 miss){
		miss = cameraObject.transform.position + cameraObject.transform.forward * placeDistance;
		return Physics.Raycast(cameraObject.transform.position, cameraObject.transform.forward, out hit, placeDistance, raycastTargets);
	}

	//creates a rotation using an up direction and a number of degrees around that axis
	Quaternion CreateDominoRotation(Vector3 localUp, float rotationAngle){
		Vector3 localForward = Quaternion.FromToRotation(Vector3.up, localUp) * Vector3.forward;
		localForward = Quaternion.AngleAxis(rotationAngle, localUp) * localForward;
		return Quaternion.LookRotation(localForward, localUp);
	}

	//returs a point hovering over the hit location
	Vector3 SmartPlacePoint(RaycastHit hit){
		return hit.point + hit.normal * placementIndicator.transform.localScale.y / 2;
	}

    //=============================================================================================
	// placement indicator adjustments
    //=============================================================================================

	//moves the placement indicator
	void AdjustIndicatorPosition(Vector3 point){
		placementIndicator.transform.position = point;
	}

	//changes the indicator's material
	void AdjustIndicatorColor(bool canPlace){
		Material toSwitch = canPlace ? canPlaceMaterial : noPlaceMaterial;
		if(indicatorRenderer.material != toSwitch)	//not sure if this saves any processing power
			indicatorRenderer.material = toSwitch;
	}

	//changes the indicator's visibility
	void AdjustIndicatorVisibility(bool visible){
		indicatorRenderer.enabled = visible;
	}

	//changes the indicator's orientation
	void AdjustIndicatorRotation(Quaternion rotation){
		placementIndicator.transform.rotation = rotation;
	}

    //=============================================================================================
	// deletion indicator functions
    //=============================================================================================

	//sets the targetToDelete's material back to what it was before it was selected
	void DelRestoreDominoAppearance(){
		if (targetToDelete != null)
			targetToDelete.GetComponent<MeshRenderer>().material = targetOldMaterial;
	}

	//stops keeping track of the targetToDelete and restores its appearance
	void DelAbandonDomino(){
		DelRestoreDominoAppearance();
		targetToDelete = null;
	}

	//chooses a new domino to be the delete target and changes its material to the noPlace material
	void DelAdoptDomino(GameObject newTarget){
		if (targetToDelete == newTarget)
			return;
		DelAbandonDomino();
		if (newTarget == null)
			return;
		targetToDelete = newTarget;
        MeshRenderer mRenderer = targetToDelete.GetComponent<MeshRenderer>();
		targetOldMaterial = mRenderer.material;
		mRenderer.material = noPlaceMaterial;
	}

    //=============================================================================================
	// network spawning and despawning
    //=============================================================================================

    /// <summary>
    /// This command instantiates the placement indicator for the player,
    /// gives authority over it to the player, spawns it for all clients,
    /// and finally sends back a reference to the object for the player to use.
    /// 
    /// This version does the instantiation on the server; next version
    /// instantiates on the client
    /// </summary>
	[Command]
    void CmdSpawnIndicator(){
        RpcDebugOutput("CmdSpawnIndicator with conn: " + connectionToClient);
        placementIndicator = Instantiate(placementIndicatorPrefab);
        indicatorRenderer = placementIndicator.GetComponent<MeshRenderer>();
        placementIndicator.transform.localScale = dominoPrefab.transform.localScale;
        placementIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");

        NetworkServer.SpawnWithClientAuthority(placementIndicator, connectionToClient);

        TargetSpawnIndicator(connectionToClient, placementIndicator);
	}

    //this is the part of the above functionality that sends the reference to
    //the game object back to the player
    [TargetRpc]
    void TargetSpawnIndicator(NetworkConnection target, GameObject spawnedIndicator) {
        print("TargetSpawnIndicator with conn: " + target);
        placementIndicator = spawnedIndicator;
        indicatorRenderer = placementIndicator.GetComponent<MeshRenderer>();
    }

    [ClientRpc]
    void RpcDebugOutput(string text) {
        print(text);
    }

	[Command]
	void CmdSpawnDomino(Vector3 point, Quaternion rotation, Vector3 gravity, GameObject onBlock){
		DominoGravity grav = Instantiate(dominoPrefab, point, rotation);
		NetworkServer.Spawn(grav.gameObject);
		grav.ServerSetGravity(gravity);
		grav.RpcSetGravity(gravity);

        //this part gives the face the domino was placed on a reference to the domino
        //so that if the block is deleted the domino can be removed too
        //note that this is server-side only
        BlockFace face = onBlock.GetComponent<BlockFace>();
        if (face != null) {
            face.AddDomino(grav.gameObject);   //give the block a reference to the domino
            grav.GetComponent<BlockLink>().SetLink(face);  //and give the domino a reference to the block
        }
	}

    [Command]
    void CmdSetActive(GameObject obj, bool active) {
        obj.SetActive(active);
        RpcSetActive(obj, active);
    }

    [ClientRpc]
    void RpcSetActive(GameObject obj, bool active) {
        obj.SetActive(active);
    }

	[Command]
	void CmdDestroy(GameObject toDelete){
		NetworkServer.Destroy(toDelete);
	}

    //=============================================================================================
    // UI methods
    //=============================================================================================

    public string ModeName() {
        return modeName;
    }
}
