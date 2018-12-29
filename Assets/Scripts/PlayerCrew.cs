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

    /////////////////////////////////////////////////////////////////////////////
    //  NETWORKING INITIALIZATION

    public void setPlayerReferences(uint ownerIdNum, int shipIdNum)
    {
        CmdSetOwnerRef(ownerIdNum);     //  will propogate to all existing clients
        CmdSetShipRef(shipIdNum);       //  will propogate to all existing clients


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
        showInterior();
        playerOnShip = true;
    }

    //  Update all clients according to their serverside version (data that isn't regularly updated)
    [Command]
    void CmdBroadcastDataFromAllCrew()
    {
        GameObject[] allCrew = GameObject.FindGameObjectsWithTag("PlayerCrew");

        for(int i = 0; i < allCrew.Length; i++)
        {
            // access this PlayerCrew's data from server, propogate to all clients
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

    
    /////////////////////////////////////////////////////////////////////////////////
    //  UPDATE AND PLAYER CONTROL
    // Update is called once per frame
    void Update () {
        if(!playerOnShip && playerConnRef!= null && playerShipRef != null)
        {
            CmdBroadcastDataFromAllCrew(); // update OTHER crew objects on this computer according to server (only data that isn't regularly broadcast)
            putPlayerOntoShip(); // references are good to go, put player onto ship on THIS computer

        }

        if (hasAuthority)
        {
            correctShipVacancy();

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

    void correctShipVacancy()
    {
        ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();
        if(shipScriptRef.pilotControlEnabled && !shipScriptRef.pilotSeatOccupied)
        {
            shipScriptRef.setPilotSeatOccupied(true);
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
        setExteriorControls(true);
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
        Debug.Log("Switching to interior mode");

        // Disable exterior controls
        setExteriorControls(false);
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

    void setExteriorControls(bool enabled)  
    {
        // assumes attempting pilot control access
        // assumes no other players trying to control ship (if multiple, won't fail, but will have competing forces acting on ship)
        ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();

        

        if (enabled) // if attempting to assume pilot control
        {
            if (!shipScriptRef.pilotSeatOccupied) // if pilot seat is NOT occupied
            {
                CmdSetShipAuthority();
                shipScriptRef.pilotControlEnabled = true;
            }
        }
        else  // if attempting to LEAVE external perspective
        {
            if (shipScriptRef.pilotControlEnabled)  // check if we HAD control
            {
                shipScriptRef.setPilotSeatOccupied(false);  // broadcast that pilot seat is vacant
                shipScriptRef.pilotControlEnabled = false;  // disable control on THIS computer
            }
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

    //  Remove client authority from any user who has authority
    //  then, give authority to the owner of this PlayerCrew object
    [Command]
    void CmdSetShipAuthority()
    {
        ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();

        NetworkIdentity newPlayerID = playerConnRef.GetComponent<NetworkIdentity>();
        NetworkIdentity shipID = playerShipRef.GetComponent<NetworkIdentity>();
        NetworkConnection otherShipOwner = shipID.clientAuthorityOwner;

        if (otherShipOwner != newPlayerID.connectionToClient)
        {
            if (otherShipOwner != null)
            {
                shipID.RemoveClientAuthority(otherShipOwner);
            }
            shipID.AssignClientAuthority(newPlayerID.connectionToClient);
            shipScriptRef.setPilotSeatOccupied(true);
        }

    }

    

    //[Command]
    //void CmdGiveShipAuthority()
    //{
    //    ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();
    //    //shipScriptRef.pilotSeatOccupied = true;
    //    shipScriptRef.setPilotSeatOccupied(true);

    //    // Client that called this command will have authority over ship
    //    NetworkIdentity playerID = playerConnRef.GetComponent<NetworkIdentity>();
    //    playerShipRef.GetComponent<NetworkIdentity>().AssignClientAuthority(playerID.connectionToClient);
    //}

    //[Command]
    //void CmdDisableShipAuthority()
    //{
       
    //    ShipMovement shipScriptRef = playerShipRef.GetComponent<ShipMovement>();

    //    if (shipScriptRef.pilotControlEnabled)
    //    {
    //        // shipScriptRef.pilotSeatOccupied = false;
    //        shipScriptRef.setPilotSeatOccupied(false);

    //        NetworkIdentity playerID = playerConnRef.GetComponent<NetworkIdentity>();
    //        playerShipRef.GetComponent<NetworkIdentity>().RemoveClientAuthority(playerID.connectionToClient);
    //    }
    //}

    ////////////////////////////////////////////////////////////////////////////
    //  SHIP DEATH

    void shipDeath()
    {
        //  destroy ship exterior
        //  destroy ship interior
        //  destroy this 
    }
}