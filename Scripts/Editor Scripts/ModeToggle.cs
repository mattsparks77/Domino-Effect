using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//change this to use an array of arrays of varying lengths, each containing
//different behaviors. 

public class ModeToggle : MonoBehaviour {

    [SerializeField] KeyCode switchKey = KeyCode.Tab;

    DominoSpawnerTwo dominoSpawner;
    BlockPlacement blockSpawner;
    BlockFaceEditor faceEditor;
    int mode = 0;

	// Use this for initialization
	void Start () {
        dominoSpawner = GetComponent<DominoSpawnerTwo>();
        blockSpawner = GetComponent<BlockPlacement>();
        faceEditor = GetComponent<BlockFaceEditor>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(switchKey))
            SwitchMode();
	}

    void SwitchMode() {
        mode = (mode + 1) % 3;
        if (mode == 0) {
            dominoSpawner.enabled = false;
            blockSpawner.enabled = true;
            faceEditor.enabled = false;
        } else if (mode == 1) {
            dominoSpawner.enabled = true;
            blockSpawner.enabled = false;
            faceEditor.enabled = false;
        } else {
            dominoSpawner.enabled = false;
            blockSpawner.enabled = false;
            faceEditor.enabled = true;
        }
    }
}
