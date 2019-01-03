using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour {

    public GameObject PlayerObjPrefab;

    public GameObject camRef;

    public GameObject playerCrewPrefab;
    public GameObject myCrew;


    public GameObject PlayerShipObj;

    public GameObject shipSelectionMenuPrefab;
    private GameObject shipSelectMenu;


    public GameObject networkManagerRef;

    private bool nameInitialized = false;

    [SyncVar]
    private short myShipNum;

    public string textBoxString = "player";

    //public Text nameLabelRef;
    

    private int boxHeight = 0;

    void OnGUI()
    {
       // if(isLocalPlayer)
            textBoxString = GUI.TextField(new Rect(50, boxHeight, 100, 30), textBoxString);
    }

	// Use this for initialization
	void Start () {

        setTextBoxUI();
        Debug.Log("PlayerConnection's start() method began");

        networkManagerRef = GameObject.Find("NetworkManager");
        //  Open ship selection menu
        if (isLocalPlayer)
        {
            Debug.Log("We are local player");
            showShipSelectionMenu();
        }
		
	}


    //////////////////////////////////////////////////////////////////////////////
    //  SELECTING A SHIP


    void showShipSelectionMenu()
    {
        Debug.Log("Attempting to show selection menu");
        shipSelectMenu = Instantiate(shipSelectionMenuPrefab);
        shipSelectMenu.GetComponent<JoinShipMenu>().ownerRef = gameObject;
    }

    //  called by button
    public void createPlayerOnShip(int shipNum)
    {
        CmdCreatePlayerOnShip(shipNum);
    }


    [Command]
    void CmdCreatePlayerOnShip(int shipNum)
    {
        string findShip = shipName(shipNum);
        myShipNum = (short)shipNum;

        Debug.Log("Joining ship: " + findShip);

        //  Spawn this client's playerCrew on all clients
        //  Set crew on the correct ship
        //  set back and forth references between owner and crew on ALL clients
        myCrew = Instantiate(playerCrewPrefab);
        PlayerCrew crewScriptRef = myCrew.GetComponent<PlayerCrew>();
        //crewScriptRef.ownerId = netId.Value;
        //crewScriptRef.myShipId = shipNum;

        //myCrew.GetComponent<PlayerCrew>().setOwnerRef(netId.Value);
        NetworkServer.SpawnWithClientAuthority(myCrew, connectionToClient); // give authority over spawned object to the client that called this command

        crewScriptRef.setPlayerReferences(netId.Value, shipNum); // current function body is on the server, therefore this line will execute only on server
    }

    

    



    string shipName(int shipNum)
    {
        string findShip = "noship";
        if (shipNum == 0)
        {
            findShip = "BlueShip";
        }
        else if (shipNum == 1)
        {
            findShip = "RedShip";
        }
        return findShip;
    }


    void setTextBoxUI()
    {
        if (isLocalPlayer)
        {
            boxHeight = Screen.height - 50;

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

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("value of pname: " + textBoxString);
                CmdChangePlayerShipName(textBoxString);
                PlayerShipObj.GetComponent<ShipMovement>().setDisplayName(gameObject.name);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                //if(PlayerShipObj != null)
                //{
                //    ShipCombat combatRef = PlayerShipObj.GetComponent<ShipCombat>();
                //    combatRef.explode(50); // explosion duration of 50 fixedupdates
                //}
                //CmdSpawnMyShip();

            }

        }

    }


    ///////////////////////////////////////////////////////////////////////////////////////////
    //  PLAYER LOADS IN



    
	
	

    //  ===================================================================================================
    //  **************************     UPDATE NAMES UPON CLIENT INITIALIZATION      ***********************
    //  ===================================================================================================

    public void updateDataInit()
    {
        //Debug.Log("Entered client's PlayerConnection request");
        //CmdUpdateDataInit();
    }

    //  When client loads into server, he requests data of all other gameobjects from server
    //  This function loops through every game object ON THE SERVER, and broadcasts data from each one.
    //[Command]
    //void CmdUpdateDataInit()
    //{
    //    Debug.Log("Server beginning to process request...");

    //    GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

    //    for(int i = 0; i < allPlayers.Length; i++)
    //    {

    //        // update names
    //        PlayerConnection playerRef = allPlayers[i].GetComponent<PlayerConnection>();
    //        ShipMovement playerShipRef = playerRef.PlayerShipObj.GetComponent<ShipMovement>();
    //        if (playerShipRef != null)
    //        {

    //            playerRef.RpcUpdateNamesInit(playerRef.name);
    //            playerShipRef.setDisplayName(playerRef.name);

    //            //  Update HP
    //            ShipCombat playerCombatRef = playerRef.PlayerShipObj.GetComponent<ShipCombat>();
    //            Debug.Log("Player " + allPlayers[i].name + "'s hp on server: " + playerCombatRef.currentHP);
    //            playerCombatRef.RpcSetHealth(playerCombatRef.currentHP);
    //            playerCombatRef.RpcUpdateHealthBar(playerCombatRef.currentHP);
    //        }

    //        //  Add more 'paragraphs' as more data types get added

    //    }

    //    //RpcUpdateNamesInit(gameObject.name);
    //}

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
    void CmdSpawnMyShip()
    {

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

    

    


}
