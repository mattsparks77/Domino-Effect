using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SingletonSupport : NetworkBehaviour {

	public HashSet<Vector3> playerPlacementDirections = new HashSet<Vector3>{
		Vector3.down, Vector3.left, Vector3.back
	};
	public bool fixedGravityMode = false;
	public bool allowSpawn = true;

}
