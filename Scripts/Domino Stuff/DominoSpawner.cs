using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//goes on player parent
public class DominoSpawner : NetworkBehaviour {

	[SerializeField] Camera cameraObject;
	[SerializeField] DominoGravity dominoPrefab;
	[SerializeField] GameObject ghostInstance;
	[SerializeField] Material badGhostMaterial;
	[SerializeField] Vector3 gravityDirection = Vector3.down;
	[SerializeField] float surfaceTolerance = 2f;
	[SerializeField] LayerMask raycastTargets;
	[SerializeField] LayerMask spawnTargets;
	[SerializeField] LayerMask dominoTargets;
	[SerializeField] float rotationSensitivity = 2f;
	[SerializeField] float placeDistance = 10f;

	SingletonSupport supporter;

	float currentRotationAngle = 0f;
	OverlapDetector detector;
	MeshRenderer ghostMesh;
	Material goodGhostMaterial;
	Vector3 targetPoint;
	Vector3 targetNormal;
	DominoSpawnBehavior currentMode = DominoSpawnBehavior.Hover;
	GameObject deleteTarget = null;

	enum DominoSpawnBehavior{
		None, Spawn, Delete, Hover
	}

	void Awake(){
		supporter = FindObjectOfType<SingletonSupport>();
		if(supporter != null && !supporter.fixedGravityMode)
			TakeAvailablePlacement();
		if (ghostInstance != null){
			ghostInstance.transform.SetParent(null);
			ghostMesh = ghostInstance.GetComponentInChildren<MeshRenderer>();
			goodGhostMaterial = ghostMesh.material;
			detector = ghostInstance.GetComponent<OverlapDetector>();
		}

		//rotate the player object - hopefully it works without issue
		transform.up = -gravityDirection;
	}

	void TakeAvailablePlacement(){
		foreach (Vector3 v in supporter.playerPlacementDirections){
			gravityDirection = v;
			supporter.playerPlacementDirections.Remove(v);
			return;
		}
		throw new KeyNotFoundException("No elements in placement direction set to pick!");
	}

	void OnDestroy(){
		RelinquishPlacement();
	}

	void RelinquishPlacement(){
		supporter.playerPlacementDirections.Add(gravityDirection);
	}

	void Update () {
		if (!isLocalPlayer)
			return;
		RotateDomino();
		UpdateTarget();
		PlaceGhost();
		CheckOverlap();
		SetGhostColor();
		if (Input.GetMouseButtonUp(0) && (supporter == null || supporter.allowSpawn)){
			DoDomino();
		}
	}

	void DoDomino(){
		switch (currentMode){
			case DominoSpawnBehavior.Spawn:
				TrySpawnDomino();
				break;
			case DominoSpawnBehavior.Delete:
				TryDeleteDomino();
				break;
		}
		
	}

	void RotateDomino(){
		float scroll = Input.mouseScrollDelta.y;
		currentRotationAngle += scroll * rotationSensitivity;
		currentRotationAngle = currentRotationAngle % 360;
	}

	bool AdaptiveRaycast(out RaycastHit hit, out DominoSpawnBehavior behavior){
		behavior = DominoSpawnBehavior.None;
		if (Physics.Raycast(cameraObject.transform.position, cameraObject.transform.forward, out hit, placeDistance, raycastTargets)){
			if (((1 << hit.collider.gameObject.layer) & spawnTargets.value) != 0){
				//hit a spawnable area
				if (Vector3.Angle(gravityDirection * -1, hit.normal) <= surfaceTolerance
					|| Vector3.Angle(gravityDirection, hit.normal) <= surfaceTolerance){
					behavior = DominoSpawnBehavior.Spawn;
				}
			} else if (((1 << hit.collider.gameObject.layer) & dominoTargets.value) != 0){
				//hit a domino
				DominoGravity targetGravScript = hit.collider.gameObject.GetComponentInParent<DominoGravity>();
				if(!targetGravScript.CanBeDeleted()){
					behavior = DominoSpawnBehavior.None;
					return true;
				}
				Vector3 targetGravity = targetGravScript.Gravity;
				float angleOffset = Vector3.Angle(targetGravity, gravityDirection.normalized);
				if (angleOffset <= surfaceTolerance || angleOffset >= 180 - surfaceTolerance){
					deleteTarget = hit.collider.gameObject;
					behavior = DominoSpawnBehavior.Delete;
				}
			}
			return true;
		} else{
			behavior = DominoSpawnBehavior.Hover;
		}
		return false;
	}

	void UpdateTarget(){
		RaycastHit hit;
		if (AdaptiveRaycast(out hit, out currentMode)){
			targetPoint = hit.point;
			targetNormal = hit.normal;
		}
	}

	void SetGhostColor(){
		if (currentMode == DominoSpawnBehavior.Spawn)
			ghostMesh.material = goodGhostMaterial;
		else
			ghostMesh.material = badGhostMaterial;
	}

	Quaternion DominoRotation(bool reversed){
		return Quaternion.FromToRotation(Vector3.up, -gravityDirection * (reversed?1:-1))
			* Quaternion.AngleAxis(currentRotationAngle * (reversed?1:-1), Vector3.up);
	}

	void SetTransformToDominoPlacement(Transform t, bool reversed){
		t.rotation = DominoRotation(reversed);
	}

	void PlaceGhost(){
		SetTransformToDominoPlacement(ghostInstance.transform, Vector3.Angle(targetNormal, gravityDirection) > 90f);
		if (currentMode != DominoSpawnBehavior.Hover){
			ghostInstance.transform.position = targetPoint;
		} else{
			ghostInstance.transform.position = cameraObject.transform.position + cameraObject.transform.forward * placeDistance;
		}
	}

	void CheckOverlap(){
		if (detector.IsOverlapping() && currentMode == DominoSpawnBehavior.Spawn){
			currentMode = DominoSpawnBehavior.None;
		}
	}

	void TryDeleteDomino(){
		if (deleteTarget != null){
			CmdDestroyDomino(deleteTarget);
		}
	}

	void TrySpawnDomino(){
		CmdSpawnDomino(targetPoint, targetNormal, gravityDirection);
	}

	[Command]
	void CmdSpawnDomino(Vector3 point, Vector3 normal, Vector3 gravity){
		bool reversed = Vector3.Angle(normal, gravity) > 90f;
		DominoGravity grav = Instantiate(dominoPrefab, detector.ColliderTransform().position, detector.ColliderTransform().rotation);
		NetworkServer.Spawn(grav.gameObject);
		grav.ServerSetGravity(gravity * (reversed?1:-1));
		grav.RpcSetGravity(gravity * (reversed?1:-1));
	}

	[Command]
	void CmdDestroyDomino(GameObject toDelete){
		NetworkServer.Destroy(toDelete);
	}
}
