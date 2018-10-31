using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoChain : MonoBehaviour {


    public bool hitByDomino = false;


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Domino" && collision.gameObject.GetComponent<DominoChain>().hitByDomino)
        {
            hitByDomino = true;
            //Debug.Log("Domino Chain");
        }

    }
}
