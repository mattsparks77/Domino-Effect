using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDetector : MonoBehaviour {

	List<KeyValuePair<SoundEmitter, float>> perceivedVolumes = new List<KeyValuePair<SoundEmitter, float>>();
	bool wasUpdatedThisFrame = false;

	void FixedUpdate(){
		wasUpdatedThisFrame = false;
	}

	float PerceivedVolume(SoundEmitter emitter){
		Vector3 displacement = emitter.transform.position - transform.position;
		return emitter.GetCurrentVolume() / (displacement.magnitude + 1);
	}

	void UpdatePerceivedVolumes(){
		perceivedVolumes.Clear();
		foreach (SoundEmitter emitter in SoundEmitter.emitters){
			float perceivedVolume = PerceivedVolume(emitter);
			perceivedVolumes.Add(new KeyValuePair<SoundEmitter, float>(emitter, perceivedVolume));
		}
		wasUpdatedThisFrame = true;
	}

	public KeyValuePair<SoundEmitter, float> GetLoudest(){
		if (!wasUpdatedThisFrame)
			UpdatePerceivedVolumes();

		KeyValuePair<SoundEmitter, float> loudest = perceivedVolumes[0];
		for (int i = 0; i < perceivedVolumes.Count; ++i){
			loudest = perceivedVolumes[i].Value > loudest.Value ? perceivedVolumes[i] : loudest;
		}

		print(loudest.Key.gameObject.name + " is making " + loudest.Value + " noise");

		return loudest;
	}
}
