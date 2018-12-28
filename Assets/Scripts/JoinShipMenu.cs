using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinShipMenu : MonoBehaviour {


    private GameObject camRef;
    public GameObject ownerRef;

    private bool killMenu = false;

	// Use this for initialization
	void Start () {
        camRef = GameObject.Find("MainCamera");
        camRef.GetComponent<CameraMovement>().snapToObject(gameObject);
        
	}


	
	// Update is called once per frame
	void Update () {
	    if(killMenu)
        {
            Destroy(gameObject);
        }	
	}


    public void doThing(int num)
    {
        Debug.Log("thing did: " + num);
        ownerRef.GetComponent<PlayerConnection>().createPlayerOnShip(num);
        killMenu = true;
    }
}
