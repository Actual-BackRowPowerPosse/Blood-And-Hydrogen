using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCrew : NetworkBehaviour {


    public GameObject playerConnRef;
    public GameObject playerShipRef;

    
    public uint ownerId;
    public int myShipId;
    bool playerOnShip = false;

	// Use this for initialization
	void Start () {
        //setOwnerRef(ownerId);
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
        transform.position = playerShipRef.GetComponent<ShipMovement>().interiorRef.transform.position;

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
            }
        }

    }

    

    // Update is called once per frame
    void Update () {
        if(!playerOnShip && playerConnRef!= null && playerShipRef != null)
        {
            CmdBroadcastDataFromAllCrew(); // update OTHER crew objects on this computer according to server (only data that isn't regularly broadcast)
            putPlayerOntoShip(); // references are good to go, put player onto ship on THIS computer

        }
        
    }


    void showExterior() //---- HOW TO HANDLE WHEN SHIP IS DEAD (i.e., when playerShipRef is null)
    {
        // Disable interior controls
        // Disable interior rendering
        //  EXTRA -- somehow slow down interior data updates? reduce client and network load somehow?

        // Enable exterior controls
        // Enable exterior rendering
        //  EXTRA -- somehow speed up data updates to normal capacity? (if it was slowed down before)

        // Set new camera behavior
    }

    void showInterior()
    {
        Debug.Log("Switching to interior mode");
    }



    //////////////////////////////////////////////////////////////////////////////////////
    //  INTERIOR

    void setInteriorControls(bool enabled)
    {

    }

    void setInteriorRendering(bool enabled)
    {
        //  access all interior objects, set render
    }

    void setInteriorCamera()
    {

    }

    //////////////////////////////////////////////////////////////////////////////////////
    //  EXTERIOR

    void setExteriorControls(bool enabled)
    {

    }

    void setExteriorRendering(bool enabled)
    {
        //  access all exterior objects, set render
    }

    void setExteriorCamera()
    {

    }


    void shipDeath()
    {
        //  destroy ship exterior
        //  destroy ship interior
        //  destroy this 
    }
}