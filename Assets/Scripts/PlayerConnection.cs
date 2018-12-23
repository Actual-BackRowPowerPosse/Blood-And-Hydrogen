using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour {

    public GameObject PlayerObjPrefab;
    public GameObject bulletPrefab;

    public GameObject camRef;

    public GameObject PlayerShipObj;

    private bool nameInitialized = false;


    public string pname = "player";

    //public Text nameLabelRef;
    

    [SyncVar]
    public short playerShipCount = 0;

    private int boxHeight = 0;

    void OnGUI()
    {
       // if(isLocalPlayer)
            pname = GUI.TextField(new Rect(50, boxHeight, 100, 30), pname);
    }

	// Use this for initialization
	void Start () {

        if (isLocalPlayer)
        {
            CmdSpawnMyShip();


            boxHeight = Screen.height - 50;


            //Debug.Log("Spawning ship: " + PlayerShipObj);
            // Set camera to look at ship
            //LinkCameraToObj(PlayerShipObj);







        }
        else
            boxHeight = Screen.height + 100;
		
	}

    // Update is called once per frame
    void Update()
    {

        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                string n = "Player (" + Random.Range(0, 100) + ")";
                CmdChangePlayerShipName(n);
                PlayerShipObj.GetComponent<ShipMovement>().setDisplayName(n);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdShootBullet();
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("value of pname: " + pname);
                gameObject.name = pname;
                PlayerShipObj.GetComponent<ShipMovement>().setDisplayName(gameObject.name);
            }

        }

    }

    public void LinkCameraToObj( GameObject obj)
    {
        if (isLocalPlayer)
        {
            camRef = GameObject.Find("MainCamera");
            camRef.GetComponent<CameraMovement>().lookAtObject(obj); // set camera to look at argument object
            
        }
    }
	
	

    //  ===================================================================================================
    //  **************************     UPDATE NAMES UPON CLIENT INITIALIZATION      ***********************
    //  ===================================================================================================

    public void UpdateNamesInit()
    {
        //Debug.Log("Entered client's PlayerConnection request");
        CmdUpdateNamesInit();
    }

    [Command]
    void CmdUpdateNamesInit()
    {
        //Debug.Log("Server beginning to process request...");

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        for(int i = 0; i < allPlayers.Length; i++)
        {
            PlayerConnection playerRef = allPlayers[i].GetComponent<PlayerConnection>();
            ShipMovement playerShipRef = playerRef.PlayerShipObj.GetComponent<ShipMovement>();
            //Debug.Log("Player " + allPlayers[i].name + " about to send info in an RPC");
            playerRef.RpcUpdateNamesInit(playerRef.name);
            playerShipRef.setDisplayName(playerRef.name);
        }

        //RpcUpdateNamesInit(gameObject.name);
    }

    [ClientRpc]
    void RpcUpdateNamesInit(string name)
    {
        if (true)
        {
            //Debug.Log("Received " + name + " name from server");
            gameObject.name = name;
            nameInitialized = true;
        }
        
    }

    /////////////////////////////////////////////  COMMANDS
    //  Initiated by a client, executed on Server side

    
    [Command]
    void CmdShootBullet()
    {
        GameObject bulletObj = Instantiate(bulletPrefab);
        bulletObj.transform.position = PlayerShipObj.transform.position;
        bulletObj.transform.rotation = PlayerShipObj.transform.rotation;
        NetworkServer.SpawnWithClientAuthority(bulletObj, connectionToClient);
    }

    [Command]
    void CmdSpawnMyShip()
    {

        playerShipCount++; // only the server will record how many playerShips it has spawned
        GameObject go = Instantiate(PlayerObjPrefab);
        PlayerShipObj = go;
        NetworkServer.SpawnWithClientAuthority(go, connectionToClient);

        

        //uint id = go.GetComponent<NetworkIdentity>().netId.Value;


        // this function executes on this networked object (by networkID) on EVERY client
        // Sets this object's internal playerShipObject reference to look at owned ship -- ship will look at "parent", too
        //RpcUpdateReferences(playerShipCount); // pass in NETWORKID of the player's ship
    }

    

    [Command]
    void CmdChangePlayerShipName(string n)
    {
        gameObject.name = n;
        RpcUpdateName(n);
    }

    //////////////////////////////////////////////////////  RPC's
    //  Initiated by server, all clients will execute on THIS networked object (by networkID) instance

    [ClientRpc]
    void RpcUpdateName(string n)
    {
        gameObject.name = n;
        nameInitialized = true;

    }

    [ClientRpc]
    void RpcUpdateReferences(short shipCount)
    {

        //Debug.Log("looking for playership");

        //GameObject[] search = GameObject.FindGameObjectsWithTag("PlayerShip(Clone)"); // create array of all spawned ships

        //for(int i = 0; i < search.Length; i++) // loop through all ships
        //{

        //    NetworkBehaviour obj = search[i].GetComponent<ShipMovement>();  // create ref to look at script

        //    if (obj.hasAuthority) // if the ship object has authority
        //        PlayerShipObj = search[i];  // set this class object's reference
        //}

        //PlayerShipObj.name = "PlayerShip (" + shipCount + ")";

        //Debug.Log("PlayerShipObj = " + PlayerShipObj);
        
        

        //if (isLocalPlayer)
        //{
        //    LinkCameraToObj(PlayerShipObj);
        //}
        
    }

    


}
