using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipMovement : NetworkBehaviour {


    public float enginesHP = 1.0f; // % decimal, 0-1 (aka .66 is 66% engine hp)
    public float thrust = 1.5f;
    public float torque = 10f;
    public float topSpeed = 10f;
    public float maxAngularVel = 10f;
    public float RAM_POWER = 10f;

    private GameObject ownerObjRef;

    private GameObject camRef;

    private Rigidbody2D rb;

    private bool camSet = false;

    private short updateCount = 0;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        //  SHIP HAS BEEN CREATED HERE
        //  LINK TO OWNER
        if(hasAuthority)
            LinkToOwner();


	}
	
	// Update is called once per frame
	void Update () {
        updateCount++;
        if (hasAuthority)
        {

            //if (!camSet)
            //{
            //    Debug.Log("Updates before setting camera: " + updateCount);
            //    LinkToOwner();
            //    camSet = true;
            //}

            float angle;
            if (Input.GetKey(KeyCode.W))        //  ----------  FORWARD
            {
                angle = rb.rotation;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                rb.AddForce(dir * thrust * enginesHP);
            }

            if (Input.GetKey(KeyCode.S))        //  ----------  BACKWARD
            {
                angle = rb.rotation;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                rb.AddForce(-dir * thrust * enginesHP);
            }

            if (Input.GetKey(KeyCode.A))        //  ----------  TURN LEFT
            {
                if (rb.angularVelocity < maxAngularVel)
                    rb.AddTorque(torque * enginesHP);
            }

            if (Input.GetKey(KeyCode.D))        //  ----------  TURN RIGHT
            {
                if (rb.angularVelocity > -maxAngularVel)
                    rb.AddTorque(-torque * enginesHP);
            }

            if (Input.GetKey(KeyCode.F))        //  ----------  TURN RIGHT
            {
                
                LinkToOwner();
            }
        }


    }

    private void LinkToOwner()
    {
        Debug.Log("PlayerShip is attempting to link to its owner");


        if (hasAuthority)
        {
            //Debug.Log("This playership has authority, attempting link...");

            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

            bool playerFound = false;

            for (int i = 0; i < allPlayers.Length && !playerFound; i++) // loop through every player
            {
                PlayerConnection playerObj = allPlayers[i].GetComponent<PlayerConnection>();
                Debug.Log("Iteration of search loop: " + i);
                if (playerObj.isLocalPlayer)
                {
                    Debug.Log("Player Found!");
                    playerObj.PlayerShipObj = gameObject;  // player object will look at THIS
                    ownerObjRef = allPlayers[i];           // THIS will look at playerobject
                    playerObj.LinkCameraToObj(gameObject);
                    playerFound = true;
                }

            }



            // camera look at this object
        }
        
    }

    


    void DestroyThisShip()
    {
        CmdDestroyShip();       //  destroy this object on Server's computer
        Destroy(gameObject);    //  Destroy this object on THIS computer
    }

    [Command]
    void CmdDestroyShip()
    {
        Destroy(gameObject);
    }
}
