using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonFunctions {

	//--------------------------------
	// placement verification
	//--------------------------------

	//gives the colliders on MASK layers that intersect with the potential block placement at point POINT
	public static Collider[] BlockOverlapColliders(Vector3 point, Vector3 boxDimensions, Quaternion boxRotation, LayerMask mask){
		Vector3 overlapDimensions = (boxDimensions / 2f) - Vector3.one * Vector3.kEpsilon;
		return Physics.OverlapBox(point, overlapDimensions, boxRotation, mask);
	}

	//whether the block can be placed here
	public static bool CanPlace(Vector3 point, Vector3 boxDimensions, Quaternion boxRotation, LayerMask placementObstructions){
		return BlockOverlapColliders(point, boxDimensions, boxRotation, placementObstructions).Length == 0;
	}

	//--------------------------------
	// raycasting
	//--------------------------------

	//does a raycast from the camera against raycast targets
	public static bool CastFromCamera(out RaycastHit hit, out Vector3 miss, Transform source, float distance, LayerMask targets){
		miss = source.position + source.forward * distance;
		return Physics.Raycast(source.position, source.forward, out hit, distance, targets);
	}

}
