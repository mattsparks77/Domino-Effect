using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TreasureCollect : MonoBehaviour {
	public Text text;
	public Text collectedText;
    public float treasure;
	public int collected = 0;
    //public Rigidbody rb;    // Use this for initialization
    public GameObject player;
    public GameObject treasureObj;
    public bool canGrab;
    void Start () {
        treasure = 0.0f;
        canGrab = false;
		text.text = "";
		collectedText.text = "Collected: ";
       // rb = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        if (canGrab)
        {
            if (Input.GetButtonDown("Pick Up"))
            {
                treasure++;
                treasureObj.SetActive(false);
            }
        }
		if (collected == 1) {
			collectedText.text = "Collected: 1 ";
		}
		else if (collected == 2) {
			collectedText.text = "Collected: 2 ";
		}
		else if (collected == 3) {
			collectedText.text = "Collected: 3 ";
		}
		if (collected == 3) {
			text.text = "You Win!!";

		}
    }


    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Treasure")
        {
            canGrab = true;
            treasureObj = collision.gameObject;
            collision.gameObject.SetActive(false);
			collected++;
        }
     
    }
    void OnTriggerExit(Collider collision)
    {
        canGrab = false;
    }
}
