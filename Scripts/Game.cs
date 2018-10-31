using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Game {

    // Use this for initialization

    
    
   public static Game current;
   public Domino[] dominos;
   public Domino start_domino;
   public Game()
        {
		
        dominos = GameObject.FindObjectsOfType<Domino>();
		Debug.Log ("Found Dominos");
    }

    }
