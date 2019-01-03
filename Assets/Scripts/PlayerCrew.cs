using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCrew : NetworkBehaviour {


    public GameObject playerConnRef;
    public GameObject playerShipRef;
    public GameObject shipInteriorRef;

    private GameObject camRef;

    private Rigidbody2D rbRef;
    
    public uint ownerId;
    public int myShipId;
    bool playerOnShip = false;

    public bool interiorControlsEnabled = false;
    public float moveSpeed;

	// Use this for initialization
	void Start () {
        //setOwnerRef(ownerId);
        rbRef = GetComponent<Rigidbody2D>();
        camRef = GameObject.Find("MainCamera");
	}


    /////////////////////////////////////////////////////////////////////////////////
    //  UPDATE AND PLAYER CONTROL
    // Update is called once per frame
    void Update()
    {

        //  if (player NOT on ship AND playerConnRef isn't set AND playerShipRef isn't set
        if (!playerOnShip && playerConnRef != null && playerShipRef != null)
        {
            CmdBroadcastDataFromAllCrew(); // server will broadcast all data from all playerCrew objects to all other clients
            putPlayerOntoShip(); // references are good to go, put player onto ship on THIS computer
            if (hasAuthority)
                showInterior();

        }

        if (hasAuthority)
        {


            if (Input.GetKeyDown(KeyCode.G))
            {
                showInterior();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                showExterior();
            }


            processMovementControls();


        }

    }

    /////////////////////////////////////////////////////////////////////////////
    //  NETWORKING INITIALIZATION

    public void setPlayerReferences(uint ownerIdNum, int shipIdNum)
    {
        CmdSetOwnerRef(ownerIdNum);     //  To server, then will propogate to all existing clients
        CmdSetShipRef(shipIdNum);       //  To server, will propogate to all existing clients


        //  POSSIBLE ERROR: by the time this method is executed, references might not be set. Consider having this method wait or triggered on client when both are set.
        //CmdPutPlayerOntoShip();     // assumes references already properly set

        
    }

    void putPlayerOntoShip()  // assumes references already properly set
    {
        // move player onto ship's interior
        Vector3 shipPosition = playerShipRef.GetComponent<ShipMovement>().interiorRef.transform.position;
        transform.position = new Vector3(shipPosition.x, shipPosition.y, transform.position.z);

        // if this is the local player
        // ...set rendering to interior
        // ...set controls to interior
        // ...set camera to interior
        // showInterior();
        playerOnShip = true;
    }

    //  Update all clients according to their serverside version (data that isn't regularly updated)
    //   - back and forth references between playerCrew object and ship/playerConnection objects
    //   - status of his ship's pilot seat vacancy
    [Command]
    void CmdBroadcastDataFromAllCrew()
    {
        GameObject[] allCrew = GameObject.FindGameObjectsWithTag("PlayerCrew");

        for(int i = 0; i < allCrew.Length; i++)
        {
            // access this PlayerCrew's data from server, propogate to all clients

            //  broadcast references again
            PlayerCrew currentCrewScriptRef = allCrew[i].GetComponent<PlayerCrew>();
            currentCrewScriptRef.setPlayerReferences(currentCrewScriptRef.ownerId, currentCrewScriptRef.myShipId);

            // broadcast some ship info
            ShipMovement shipScriptRef = currentCrewScriptRef.playerShipRef.GetComponent<ShipMovement>();
            shipScriptRef.setPilotSeatOccupied(shipScriptRef.pilotSeatOccupied);
        }

    }

    //[Command]
    //void CmdPutPlayerOntoShip()  // assumes references already properly set
    //{
    //    putPlayerOntoShip();
    //    RpcPutPlayerOntoShip();
    //}

    //[ClientRpc]
    //void RpcPutPlayerOntoShip() // assumes references already properly set
    //{
    //    putPlayerOntoShip();
    //}

    //  ---------------------------------  SETTING OWNER REFERENCE
    [Command]
    void CmdSetOwnerRef(uint ownerIdNum)
    {
        setOwnerRef(ownerIdNum);  // set reference on THIS computer (server)
        RpcSetOwnerRef(ownerIdNum); // propogate to all clients
    }

    [ClientRpc]
    void RpcSetOwnerRef(uint ownerIdNum)
    {
        setOwnerRef(ownerIdNum); // set reference on THIS computer
    }

    //  simply sets back-and-forth references between this crew and his owner
    //  ...via being given a NetID, and searching through all players for this netID
    public void setOwnerRef(uint ownerIdNum)
    {
        ownerId = ownerIdNum;

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        bool playerFound = false;
        for (int i = 0; i < allPlayers.Length && !playerFound; i++)
        {
            PlayerConnection playerRef = allPlayers[i].GetComponent<PlayerConnection>();
            if (playerRef.netId.Value == ownerIdNum)
            {
                playerConnRef = allPlayers[i];
                playerConnRef.GetComponent<PlayerConnection>().myCrew = gameObject;
                playerFound = true;
            }
        }
    }

    //  ----------------------------------------  SETTING SHIP REFERENCE

    [Command]
    void CmdSetShipRef(int myShipNum)
    {
        setShipRef(myShipNum);  // set reference on THIS computer (server)
        RpcSetShipRef(myShipNum); // propogate to all clients
    }

    [ClientRpc]
    void RpcSetShipRef(int myShipNum)
    {
        setShipRef(myShipNum); // set reference on THIS computer
    }

    //  Sets back and forth references between this crew and his ship
    //   via being given a netID, and searches for ship with this ID
    public void setShipRef(int myShipNum)
    {

        myShipId = myShipNum;

        GameObject[] allShips = GameObject.FindGameObjectsWithTag("PlayerShip");
        bool shipFound = false;
        for(int i = 0; i < allShips.Length && !shipFound; i++)
        {
            ShipMovement shipRef = allShips[i].GetComponent<ShipMovement>();
            if(shipRef.shipId == myShipNum)
            {
                playerShipRef = allShips[i];
                shipInteriorRef = allShips[i].GetComponent<ShipMovement>().interiorRef;
            }
        }

    }

    
   

    void processMovementControls()
    {
        if (interiorControlsEnabled)
        {
            if (Input.GetKey(KeyCode.W))
            {
                rbRef.velocity = new Vector2(rbRef.velocity.x, moveSpeed);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                rbRef.velocity = new Vector2(rbRef.velocity.x, -moveSpeed);
            }
            else
            {
                rbRef.velocity = new Vector2(rbRef.velocity.x, 0.0f);
            }


            if (Input.GetKey(KeyCode.A))
            {
                rbRef.velocity = new Vector2(-moveSpeed, rbRef.velocity.y);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                rbRef.velocity = new Vector2(moveSpeed, rbRef.velocity.y);
            }
            else
            {
                rbRef.velocity = new Vector2(0.0f, rbRef.velocity.y);
            }
        }
        else
            rbRef.velocity = new Vector2(0.0f, 0.0f);
    }


    void showExterior() //---- HOW TO HANDLE WHEN SHIP IS DEAD (i.e., when playerShipRef is null)
    {
        // Disable exterior controls
        setExteriorPilotControls(true);
        // disable exterior rendering
        setExteriorRendering(true);
        //  EXTRA -- somehow slow down interior dat updates? reduce client and network load somehow?


        // enable interior controls
        setInteriorControls(false);
        // enable interior rendering
        setInteriorRendering(false);


        // move camera to ship
        setExteriorCamera();
    }

    void showInterior()
    {

        // Disable exterior controls
        setExteriorPilotControls(false);
        // disable exterior rendering
        setExteriorRendering(false);
        //  EXTRA -- somehow slow down interior dat updates? reduce client and network load somehow?


        // enable interior controls
        setInteriorControls(true);
        // enable interior rendering
        setInteriorRendering(true);


        // move camera to ship
        setInteriorCamera();

    }

    void setAllRenderersFromParentTag(string tag, bool doEnable)
    {
        //  find all game objects with tag
        GameObject[] allWithTag = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < allWithTag.Length; i++)  // loop through each object found
        {
            // find all renderers within this object
            Renderer[] renderers = allWithTag[i].GetComponentsInChildren<Renderer>();
            for (int j = 0; j < renderers.Length; j++) // loop through all of those renderers
            {
                renderers[j].enabled = doEnable; // set this renderer's enabled status
            }

        }
    }



    //////////////////////////////////////////////////////////////////////////////////////
    //  INTERIOR

    void setInteriorControls(bool enabled)
    {
        interiorControlsEnabled = enabled;
    }

    void setInteriorRendering(bool enabled)
    {
        //  set rendering on all PlayerCrew objects
        setAllRenderersFromParentTag("PlayerCrew", enabled);

        //  set rendering on all ShipInterior objects
        setAllRenderersFromParentTag("ShipInterior", enabled);



    }

    

    void setInteriorCamera()
    {
        CameraMovement camScriptRef = camRef.GetComponent<CameraMovement>();
        camScriptRef.snapToObject(shipInteriorRef);
        camScriptRef.simpleLookAtObject(shipInteriorRef);
        
    }

    //////////////////////////////////////////////////////////////////////////////////////
    //  EXTERIOR

    // Entering (true) or leaving (false) pilot seat
    void setExteriorPilotControls(bool enabled)  
    {
        // assumes attempting pilot control access
        ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();

       // Debug.Log("Attempting to enter pilot seat?: " + enabled);

        if (enabled) // if attempting to assume pilot control
        {

            //Debug.Log("Yes, attempting to enter pilot seat...");

            CmdRequestPilotAuthority();
        }
        else  // if attempting to LEAVE external perspective
        {
           // Debug.Log("No, attempting to leave pilot seat");
            if (shipScriptRef.pilotControlEnabled)  // check if we HAD control
            {
               // Debug.Log("We HAD control of pilot seat, now we're setting pilot seat occupied to FALSE");
                shipScriptRef.setPilotSeatOccupied(false);  // broadcast that pilot seat is vacant
            }
            shipScriptRef.pilotControlEnabled = false;  // disable control on THIS computer
        }
    }



    void setExteriorRendering(bool enabled)
    {
        //  access all exterior objects, set render
        setAllRenderersFromParentTag("PlayerShip", enabled);
        setAllRenderersFromParentTag("Map", enabled);
        setAllRenderersFromParentTag("MapObjects", enabled);
    }

    void setExteriorCamera()
    {
        ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();
        CameraMovement camScriptRef = camRef.GetComponent<CameraMovement>();
        camScriptRef.snapToObject(playerShipRef);

        if (shipScriptRef.pilotControlEnabled)
        {
            camScriptRef.lookAtObject(playerShipRef);
        }
        else
        {
            camScriptRef.simpleLookAtObject(playerShipRef);
        }
    }

    //  This player (by netID) attempts to take over pilot seat
    //  Server-side check if pilot seat is vacant
    //  Remove client authority from any user who has authority
    //  then, give authority to the owner of this PlayerCrew object
    [Command]
    void CmdRequestPilotAuthority()
    {
        
        ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();

        //Debug.Log("Requesting pilot authority. pilotSeatOccupied is: " + shipScriptRef.pilotSeatOccupied);

        if (shipScriptRef.pilotSeatOccupied == false) // if, on serverside, pilot seat is NOT occupied
        {
            // pilot seat is vacant, give this player the pilot seat
            //Debug.Log("pilotSeatOccupied is false, therefore we are giving requesting player the pilot seat authority");
            NetworkIdentity newPlayerID = playerConnRef.GetComponent<NetworkIdentity>();
            NetworkIdentity shipID = playerShipRef.GetComponent<NetworkIdentity>();
            NetworkConnection otherShipOwner = shipID.clientAuthorityOwner;

            if (otherShipOwner != newPlayerID.connectionToClient)  //  if this player is not already set to own the ship
            {
               // Debug.Log("We are not the current owner of this pilot seat, so we will receive pilot seat authority");
                if (otherShipOwner != null)  //  if the ship actually DOES have any owner at all
                {
                    //Debug.Log("there actually was a different owner, so we are removing his authority");
                    shipID.RemoveClientAuthority(otherShipOwner);
                }
                shipID.AssignClientAuthority(newPlayerID.connectionToClient);  //  Give ownership to the requesting client
                
            }
            //else
            //{
            //    Debug.Log("We are already the owner of the ship. Doing nothing");
            //}
            shipScriptRef.setPilotSeatOccupied(true);  // communicate to all clients that pilot seat is now occupied
            RpcReceivePilotSeat();  // on proper client's computer, their ship will have pilot controls enabled
        }

    }

    //  will broadcast to this object on all computers
    //  only enable ship controls on the computer client that owns this object
    [ClientRpc]
    void RpcReceivePilotSeat()
    {

        if (hasAuthority) // check that this computer owns this object
        {

            //Debug.Log("Receiving pilot seat..");
            ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();
            shipScriptRef.pilotControlEnabled = true; // this computer will be allowed to move ship

            setExteriorCamera();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    //  SHIP DEATH

    void shipDeath()
    {
        //  destroy ship exterior
        //  destroy ship interior
        //  destroy this 
    }
}