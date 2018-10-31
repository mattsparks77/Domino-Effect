using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Potential bug: changing whether this is an end domino does not cause the
//progress tracker to re-check its count of hit end dominos against the number
//of registered end dominos

/// <summary>
/// Script that marks a domino as an objective to be knoed down.
/// 
/// I should add functionality to bind the appearance of the domino
/// to the isEndDomino state.
/// </summary>
[RequireComponent(typeof(DominoMaterial))]
public class EndChain : MonoBehaviour {

    [SerializeField] Material endMaterial;

    DominoMaterial materialManager; //will need to be set
    bool isEndDomino = false;
    public bool IsEndDomino {
        get { return isEndDomino; }
        set {
            if (value) {
                StartChain thisStart = GetComponent<StartChain>();
                if (thisStart && thisStart.IsStartDomino) {
                    thisStart.IsStartDomino = false;
                }
                materialManager.SetCorrectMaterial(endMaterial);
            } else
                materialManager.SetCorrectMaterial(null);
            isEndDomino = value;
        }
    }

	ProgressTracker tracker;
	bool hit = false;

    //will need to deal with this stuff vvvvv
    private void Awake()
    {
        materialManager = GetComponent<DominoMaterial>();
        tracker = FindObjectOfType<ProgressTracker>();
        RegisterAsEnd(isEndDomino);
    }

    void RegisterAsEnd(bool end) {
        tracker.RegisterEnder(this, end);
    }
    //^^^^^

    //sets whether this script will essentially be 'active'
    //if false, this is not an end domino
    public void SetAsEnd(bool end) {
        isEndDomino = end;
        RegisterAsEnd(end);
    }

    void OnCollisionEnter(Collision collision)
    {
		if (collision.gameObject.tag == "Domino" && !hit){
			if (collision.gameObject.GetComponent<DominoChain>().hitByDomino){
				hit = true;
                HitResponse();
			}
		}
    }

    void HitResponse() {
        if (isEndDomino)
            tracker.TargetHit();
    }

    public void Reset()
    {
		hit = false;
    }
}
