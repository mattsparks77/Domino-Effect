using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// TO DO:
/// +Show the directions that the scaling controls will operate on
/// +Add network functionality
/// 
/// The player's controls for placing and deleting blocks within the editor mode. The blocks are considered
/// to be of higher priority than other objects such as dominos and any toys that may be included in the game,
/// so placing a block over such objects will delete them.
/// 
/// Controls:
/// Left Mouse to place or delete (hardcoded for now)
/// F to switch modes - Delete and Place mode (serialized)
/// U-J to scale up or down on the vertical axis (serialized)
/// I-K to scale up or down on the horizontal axis (serialized)
/// 
/// Usage:
/// Attach to player object; it or its child should have the camera attached
/// </summary>
public class BlockPlacement : NetworkBehaviour, IEditorMode {

	//necessary referenes
	[SerializeField] EnvironmentBlock blockPrefab;
    [SerializeField] GameObject indicatorPrefab;    //this will just be a cube with a network identity
	[SerializeField] Material canPlaceMaterial;
	[SerializeField] Material noPlaceMaterial;

	[SerializeField] KeyCode modeSwitchKey = KeyCode.F;
	[SerializeField] KeyCode scaleUpVert = KeyCode.U;
	[SerializeField] KeyCode scaleDownVert = KeyCode.J;
	[SerializeField] KeyCode scaleUpHorz = KeyCode.I;
	[SerializeField] KeyCode scaleDownHorz = KeyCode.K;
	[SerializeField] float scaleAdjustSensitivity = 0.1f;
	[SerializeField][Range(1f, float.PositiveInfinity)] float minimumSize = 1f;
	[SerializeField] float placeDistAdjustSensitivity = 0.5f;
	[SerializeField] float placementDistance = 10f;
	[SerializeField] float maxPlaceDist = 30f;
	[SerializeField] float minPlaceDist = 4f;
	[SerializeField] float deleteDistance = 10f;
	[SerializeField] LayerMask castObstructions;	//objects that can block the player's interaction path
	[SerializeField] LayerMask placementObstructions;	//objects that prevent block placement
	[SerializeField] LayerMask toyLayer;	//the domino layer (but really all toy layers that should be deleted when a block is placed)
    [SerializeField] string modeName;

	//maybe also have a reference to a HUD for this

	enum BlockPlaceBehavior{
		Place, Delete
	}

    GameObject placementIndicator = null;   //can just be a cube
	MeshRenderer indicatorRenderer;
	Camera playerCam;
	Vector3 internalScale = Vector3.one;
	Vector3 boxDimensions = Vector3.one;
	BlockPlaceBehavior behavior = BlockPlaceBehavior.Place;
	EnvironmentBlock currentDeleteTarget = null;
    bool activeMode = false;


	//=============================================================================================
	// Code control
	//=============================================================================================

	void Start(){
        if (!isLocalPlayer)
            return;
		playerCam = GetComponent<Camera>();
		if (playerCam == null)
			playerCam = GetComponentInChildren<Camera>();
		if (playerCam == null)
			throw new MissingComponentException("No camera object found on or under BlockPlacement script");

        /*
		if (placementIndicator == null){
			placementIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
		} else{
			placementIndicator = Instantiate(placementIndicator);	//use the prefab
		}
		placementIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");
		indicatorRenderer = placementIndicator.GetComponent<MeshRenderer>();
		if (indicatorRenderer == null)
			throw new MissingComponentException("BlockPlacement indicator does not have a MeshRenderer component");
		*/

        CmdSpawnIndicator();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer || !activeMode)
            return;
		//ModeControl();    //this was used for local input instead of the mode manager
		if (behavior == BlockPlaceBehavior.Place)
			PlacementLogic();
		else
			DeleteLogic();
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
        if (placementIndicator != null)
            CmdDestroy(placementIndicator);
		    //Destroy(placementIndicator);
	}

    //if this is the local player, sets the indicator to be the correct visibility
    void Activate() {
        if (!isLocalPlayer)
            return;
        if (placementIndicator != null)
            SetIndicatorMode(behavior);
        //placementIndicator.SetActive(true);
        else
            CmdSpawnIndicator();
    }

    //if this is the local player, sets the indicator to be disabled
    //and relinquishes the target block
    void Deactivate() {
        if (!isLocalPlayer)
            return;
        SetDeleteTarget(null);
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

    public void RotateSubMode() {   //IEditorMode
        if (!isLocalPlayer)
            return;
        ModeControl();
    }


	//=============================================================================================
	// Mode switching
	//=============================================================================================

	//for switching whether the system is placing or deleting blocks
    //the commented code in the method was for local input instead of using the mode manager
	void ModeControl(){
		//if (Input.GetKeyDown(modeSwitchKey)){
            if (behavior == BlockPlaceBehavior.Place)
                behavior = BlockPlaceBehavior.Delete;
            else {
                SetDeleteTarget(null);
                behavior = BlockPlaceBehavior.Place;
            }
		//}
		SetIndicatorMode(behavior);
	}

	//changes the player's indicator to reflect the mode; currently just turns off the placement
	//indicator in other modes
	void SetIndicatorMode(BlockPlaceBehavior behaviorMode){
		bool showIndicator = behaviorMode == BlockPlaceBehavior.Place;
		//placementIndicator.SetActive(showIndicator);
        if(placementIndicator != null)
            CmdSetActive(placementIndicator, showIndicator);
	}

	//=============================================================================================
	// Placement behavior methods
	//=============================================================================================

	//runs the whole placement behavior
	void PlacementLogic(){
		UserChangePlaceDist();
		UserChangeBlockScale();

        //since the reference to the indicator will come from the server, there may be a delay between the
        //initialization of this object and getting the placement indicator reference back. Until we get it,
        //just wait for now.
        if (placementIndicator == null)
            return;
        
		AdjustIndicatorScale(boxDimensions);
		Vector3 placementPoint = SmartPlacementPoint();	//gets the location that a block would be placed, regardless of validity
		AdjustIndicatorPosition(placementPoint);	//moves the indicator to this location
		bool canPlace = CanPlace(placementPoint);	//checks whether this location is a valid placement point
		AdjustIndicatorColor(canPlace);	//changes how the indicator looks to inform the user whether the placement point is valid

		if (Input.GetMouseButtonDown(0) && canPlace){	//if the player clicks and a block can be placed...
			DestroyOverlappingDominos(placementPoint);	//destroy any obstructing dominos
			PlaceBlock(placementPoint);	//then spawn the block
		}
	}

	//--------------------------------
	// placement modifiers
	//--------------------------------

	//changes the block's placement distance when the player uses the scroll wheel
	void UserChangePlaceDist(){
		float scroll = Input.mouseScrollDelta.y;
		placementDistance += scroll * placeDistAdjustSensitivity;
		placementDistance = Mathf.Clamp(placementDistance, minPlaceDist, maxPlaceDist);
	}

	//changes the block's scale along the player's up and right axes
	//
	//note that this uses objective orientation, so if the block or 
	//object is allowed to rotate in the future this will not work
	void UserChangeBlockScale(){
		Vector3 verticalScaleAxis = GreatestComponent(playerCam.transform.up, true);
		Vector3 horizontalScaleAxis = GreatestComponent(playerCam.transform.right, true);

		internalScale += verticalScaleAxis * ( (Input.GetKey(scaleUpVert)?1:0) + (Input.GetKey(scaleDownVert)?-1:0) ) * scaleAdjustSensitivity;
		internalScale += horizontalScaleAxis * ( (Input.GetKey(scaleUpHorz)?1:0) + (Input.GetKey(scaleDownHorz)?-1:0) ) * scaleAdjustSensitivity;

		internalScale = MinimumScaleSize(internalScale);
		boxDimensions = RoundToGrid(internalScale);
	}

	//returns the greatest component of the source vector
	Vector3 GreatestComponent(Vector3 source, bool parallelIndicator){
		if (Mathf.Abs(source.x) > Mathf.Abs(source.y) && Mathf.Abs(source.x) > Mathf.Abs(source.z))
			return Vector3.right * (parallelIndicator?1:source.x);
		if (Mathf.Abs(source.y) > Mathf.Abs(source.z))
			return Vector3.up * (parallelIndicator?1:source.y);
		return Vector3.forward * (parallelIndicator?1:source.z);
	}

	Vector3 MinimumScaleSize(Vector3 source){
		return new Vector3(Mathf.Max(source.x, minimumSize), Mathf.Max(source.y, minimumSize), Mathf.Max(source.z, minimumSize));
	}


	//--------------------------------
	// placement indicator adjustments
	//--------------------------------

	//moves the placement indicator
	void AdjustIndicatorPosition(Vector3 point){
		placementIndicator.transform.position = point;
	}

	//changes the indicator's material based on whether a block can be placed here
	void AdjustIndicatorColor(bool canPlace){
		Material toSwitch = canPlace ? canPlaceMaterial : noPlaceMaterial;
		if(indicatorRenderer.material != toSwitch)	//not sure if this saves any processing power
			indicatorRenderer.material = toSwitch;
	}

	void AdjustIndicatorScale(Vector3 scale){
		placementIndicator.transform.localScale = scale;
	}


	//--------------------------------
	// placement effects
	//--------------------------------

	//spawns a block
	void PlaceBlock(Vector3 placementPoint){
        //EnvironmentBlock block = Instantiate(blockPrefab, placementPoint, Quaternion.identity);
        //block.transform.localScale = boxDimensions;	//may need to be changed to a function within the environment block
        CmdSpawnBlock(placementPoint, Quaternion.identity, boxDimensions);
	}

	//destroys all dominos that overlap with the block's placement (this could be modified to destroy any toys that may be placed)
	void DestroyOverlappingDominos(Vector3 placementPoint){
		Collider[] overlappingDominos = BlockOverlapColliders(placementPoint, toyLayer);
		foreach(Collider col in overlappingDominos){
			Destroy(col.gameObject);    //will need to be networked
		}
	}


	//--------------------------------
	// placement verification
	//--------------------------------

	//gives the colliders on MASK layers that intersect with the potential block placement at point POINT
	Collider[] BlockOverlapColliders(Vector3 point, LayerMask mask){
		Vector3 overlapDimensions = (boxDimensions / 2f) - Vector3.one * Vector3.kEpsilon;
		return Physics.OverlapBox(point, overlapDimensions, Quaternion.identity, mask);
	}

	//whether the block can be placed here
	bool CanPlace(Vector3 point){
		return BlockOverlapColliders(point, placementObstructions).Length == 0;
	}


	//--------------------------------
	// placement location
	//--------------------------------

	//returns the placement point for the block; if the raycast hits a target then it will be that point plus the relevant
	//dimension of the block as an offset, otherwise it will just be the end of the raycast
	Vector3 SmartPlacementPoint(){
		RaycastHit hit;	//if there is a hit, this will be it
		Vector3 spawnPoint;	//the actual point that the object will be spawned in; it is not correct immediately
		if (PlacementPoint(out spawnPoint, out hit)){
			Vector3 offset = hit.normal;
			offset.Scale(boxDimensions / 2f);
			spawnPoint = hit.point + offset;	//if there is a hit, stack the box onto the hit block
		}
		return RoundToGrid(spawnPoint);	//snap the point to the grid and return it
	}

	//gives a placement point if there is no hit, otherwise gives a hit
	bool PlacementPoint(out Vector3 rawPoint, out RaycastHit hit){
		rawPoint = Vector3.zero;
		if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, placementDistance, castObstructions)){
			return true;
		} else{
			rawPoint = playerCam.transform.position + playerCam.transform.forward * placementDistance;
			return false;
		}
	}

	//rounds a vector3 to the nearest grid point
	//
	//this could be changed to handle any grid size! Not just size one.
	Vector3 RoundToGrid(Vector3 point){
		return new Vector3(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), Mathf.RoundToInt(point.z));
	}


	//=============================================================================================
	// Delete behavior methods
	//=============================================================================================

	//runs the whole deletion behavior
	void DeleteLogic(){
		EnvironmentBlock newTarget = FindTargetBlock();	//try to find a block
		SetDeleteTarget(newTarget);

		if (currentDeleteTarget == null)	//no block detected?
			return;
		
		if (Input.GetMouseButtonDown(0)){
			DeleteBlock(currentDeleteTarget);
		}
		
	}

	//tries to find a target that could be deleted
	EnvironmentBlock FindTargetBlock(){
		RaycastHit hit;
		if (!Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, placementDistance, castObstructions))	//facing anything?
			return null;
		else
			return hit.collider.gameObject.GetComponent<EnvironmentBlock>();
	}

	//sets the delete target and changes the look of the old and new targets to indicate which is being selected
	void SetDeleteTarget(EnvironmentBlock newTarget){
		if (currentDeleteTarget == newTarget)	//if it's the same target, don't worry about it
			return;
		//otherwise, change this target's color back to normal
		if(currentDeleteTarget != null)
			currentDeleteTarget.EditorChangeMaterial(null);

		//and set the new target's color to the indicator color if it isn't null
		if(newTarget != null)
			newTarget.EditorChangeMaterial(noPlaceMaterial);

		//finally set the current target to the new target
		currentDeleteTarget = newTarget;
	}

	//destroys a block
	void DeleteBlock(EnvironmentBlock block){
		//Destroy(block.gameObject);
        CmdDestroy(block.gameObject);
	}


    //=============================================================================================
    // Network methods
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
    void CmdSpawnIndicator() {
        RpcDebugOutput("CmdSpawnIndicator with conn: " + connectionToClient);
        placementIndicator = Instantiate(indicatorPrefab);
        indicatorRenderer = placementIndicator.GetComponent<MeshRenderer>();
        placementIndicator.transform.localScale = blockPrefab.transform.localScale;
        placementIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");

        NetworkServer.SpawnWithClientAuthority(placementIndicator, connectionToClient);
        //NetworkServer.Spawn(placementIndicator);
        //NetworkIdentity identity = placementIndicator.GetComponent<NetworkIdentity>();
        //identity.localPlayerAuthority = true;
        //identity.AssignClientAuthority(connectionToClient);

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

    [Command]   //I wonder; is it better practice to pass in the prefab anyway?
    void CmdSpawnBlock(Vector3 point, Quaternion rotation, Vector3 scale) {
        EnvironmentBlock block = Instantiate(blockPrefab, point, rotation);
        block.transform.localScale = scale;
        NetworkServer.Spawn(block.gameObject);
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
    void CmdDestroy(GameObject toDelete) {
        NetworkServer.Destroy(toDelete);
    }

    //=============================================================================================
    // UI methods
    //=============================================================================================

    public string ModeName() {
        return modeName;
    }
}
