using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//goes on the root of the ghost domino
public class DominoFocusedPlayer : NetworkBehaviour {

	[SerializeField] Camera cameraObject;
	[SerializeField] DominoGravity dominoPrefab;
	[SerializeField] GameObject dominoGhost;
	[SerializeField] Material badGhostMaterial;
	[SerializeField] Vector3 gravityDirection = Vector3.down;
	[SerializeField] float surfaceTolerance = 2f;
	[SerializeField] LayerMask spawnTargets;
	[SerializeField] LayerMask dominoTargets;
	[SerializeField] float rotationSensitivity = 2f;
	[SerializeField] float placeDistance = 10f;

	float currentRotationAngle = 0f;
	GameObject ghostInstance;
	MeshRenderer ghostMesh;
	Material goodGhostMaterial;
	Vector3 targetPoint;
	Vector3 targetNormal;
	DominoSpawnBehavior currentMode = DominoSpawnBehavior.Hover;

	enum DominoSpawnBehavior{
		None, Spawn, Delete, Hover
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
