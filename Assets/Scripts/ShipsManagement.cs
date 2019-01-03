using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipsManagement : NetworkBehaviour {

    public GameObject shipExteriorPrefab;
    public GameObject shipInteriorPrefab;


    public GameObject[] allShips;
    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void AddShip(short team)  // 0 -- blue team.  1 -- Red team
    {
        //  create 
    }

    
}
