using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapDetector : MonoBehaviour {

	[SerializeField] LayerMask detectedColliders;
	[SerializeField] BoxCollider dominoCollider;

	public bool IsOverlapping(){
		Vector3 correctSize = Vector3.Scale(dominoCollider.size, dominoCollider.transform.localScale);
		return Physics.CheckBox(dominoCollider.transform.position, correctSize/2, dominoCollider.transform.rotation, detectedColliders);
	}

	public Transform ColliderTransform(){
		return dominoCollider.transform;
	}
}
