using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour {

	public static List<SoundEmitter> emitters = new List<SoundEmitter>();

	[SerializeField] float speedFactor = 1f;
	[SerializeField] float weightFactor = 1f;

	TreasureCollect collector;
	Rigidbody body;

	void Awake(){
		collector = GetComponent<TreasureCollect>();
		body = GetComponent<Rigidbody>();
	}

	void OnEnable(){
		emitters.Add(this);
	}

	void OnDisable(){
		emitters.Remove(this);
	}

	public float GetCurrentVolume(){
		//will switch to references when I have them
		Vector3 velocity = body != null ? body.velocity : Vector3.zero;
		float volume = SpeedComponent(velocity) * WeightComponent(1);
		return volume;
	}

	float SpeedComponent(Vector3 velocity){
		return velocity.magnitude * speedFactor;
	}

	float WeightComponent(float weight){
		return weight * weightFactor;
	}
}
