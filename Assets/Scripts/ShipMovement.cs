using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ShipMovement : NetworkBehaviour {


    public float enginesHP = 1.0f; // % decimal, 0-1 (aka .66 is 66% engine hp)
    public float thrust = 1.5f;
    public float torque = 10f;
    public float topSpeed = 10f;
    public float maxAngularVel = 10f;
    public float RAM_POWER = 12f; //testing some changes lool

    public int shipId;
    public GameObject interiorRef;

    public GameObject childUI;


    public Text nameLabelRef;

    //  change this to "current pilot"
    private GameObject currentPilotRef;

    private GameObject camRef;

    private Rigidbody2D rb;

    public string displayName;


    //private bool shipShown = true;  // has a get() method
    public bool pilotControlEnabled = false;

    private bool camSet = false;

    private short updateCount = 0;

    
    public bool pilotSeatOccupied; // should be networked via the setPilotSeatOccupied command

    ///////////////////////////////////////////////////////////////////////////
    //  PILOT SEAT VACANCY NETWORKING MANAGEMENT

    public void setPilotSeatOccupied(bool isOccupied)
    {
        CmdSetPilotSeatOccupied(isOccupied);
    }

    [Command]
    void CmdSetPilotSeatOccupied(bool isOccupied)
    {
        //Debug.Log("on Serverside, Setting pilot seat occupied to: " + isOccupied);
        pilotSeatOccupied = isOccupied;
    }

    
    //------------------------------------------------------------------------
    

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        gameObject.layer = 10; // networkships
        //  SHIP HAS BEEN CREATED HERE
        //  LINK TO OWNER
        //if(hasAuthority)
        //    LinkToOwner();
        Quaternion rotation = new Quaternion(0.0f, 0.0f, gameObject.transform.rotation.z, 0.0f);
        childUI.transform.rotation = rotation;

        setDisplayName("DefaultShip");


    }
    

    void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("G key press detected");
                bool shipShown = GetComponent<SpriteRenderer>().enabled;
                shipShown = !shipShown;
                setShipRenderers(shipShown);
            }
        }

        

    }
	
	// Update is called once per frame
	void FixedUpdate () {
        updateCount++;
        //Vector3 rotation = new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y, gameObject.transform.rotation.z * -1);
        Quaternion rotation = new Quaternion(0.0f, 0.0f, gameObject.transform.rotation.z, 0.0f);
        childUI.transform.rotation = rotation;

       
        //if (hasAuthority)
        //{

        //    if (!camSet)
        //    {
        //        initializePlayer();
        //    }

            
        //}

        processInputs();


    }

    //  From server, broadcasts all data from every ship object
    //  ...so, every ship object for every client will be updated according...
    //  ...to its version on the server
    void CmdUpdateDataAllShips()
    {

        GameObject[] allShips = GameObject.FindGameObjectsWithTag("PlayerShip");

        // loop through each ship object
        for(int i = 0; i < allShips.Length; i++)
        {
            //  for each ship object, broadcast all data from server
            ShipMovement shipScriptRef = allShips[i].GetComponent<ShipMovement>();

            //  ship name
            shipScriptRef.setDisplayName(shipScriptRef.displayName);
            
            //  ...add more paragraphs as more data values need to be broadcast

        }

    }


    //  Sets back and forth references between this world-map ship object and the interior-view ship object
    //  searches through all interior objects for the one with matching shipID
    void linkToInterior()
    {

        Debug.Log("Linking ship to interior: " + gameObject.name);

        GameObject[] allInteriors = GameObject.FindGameObjectsWithTag("ShipInterior");
        bool interiorFound = false;


        for(int i = 0; i < allInteriors.Length && !interiorFound; i++)
        {
            Debug.Log("Checking ship's Id: " + allInteriors[i].name);
            ShipInterior currentInteriorRef = allInteriors[i].GetComponent<ShipInterior>();
            if(currentInteriorRef.shipId == shipId)
            {
                interiorRef = allInteriors[i];
                currentInteriorRef.shipExteriorRef = gameObject;
                interiorFound = true;
                Debug.Log("Ship found its interior: " + allInteriors[i].name);
            }


        }

    }



    
    void processInputs()
    {
        if (pilotControlEnabled)
        {
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
        }
    }

    
    //void ApplyShipThrust(bool thrustForward)
    //{
    //    float angle;
    //    angle = rb.rotation;
    //    Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
    //    if (!thrustForward)
    //        dir *= -1; // send thrust backwards
    //    rb.AddForce(dir * thrust * enginesHP);
    //}

    //void ApplyShipTorque(bool isClockwise)
    //{

    //    if (isClockwise)
    //    {
    //        if (rb.angularVelocity > -maxAngularVel)
    //            rb.AddTorque(-torque * enginesHP);
    //    }
    //    else // turn counterclockwise
    //    {
    //        if (rb.angularVelocity < maxAngularVel)
    //            rb.AddTorque(torque * enginesHP);
    //    }
    //}

    

    void setShipRenderers(bool doRender)
    {
        Debug.Log("Setting ship visibility to: " + doRender);
        Renderer[] renderers;
        renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (short i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = doRender;
        }
    }

    void initializePlayer()
    {
        Debug.Log("Updates before setting camera: " + updateCount);
        camSet = true;
        gameObject.layer = 8; //localShip
        gameObject.GetComponent<ShipCombat>().currentGunnerRef = currentPilotRef;
        
    }
    
    //private void updateDataInit()
    //{
    //    //Debug.Log(gameObject.name + " is requesting playernames from server");

    //    currentPilotRef.GetComponent<PlayerConnection>().updateDataInit();
    //}

    public void LinkCameraToObj(GameObject obj)
    {
        if (hasAuthority)
        {
            camRef = GameObject.Find("MainCamera");
            //camRef.GetComponent<CameraMovement>().lookAtObject(obj); // set camera to look at argument object

        }
    }



    

    public void setDisplayName(string name)
    {
        displayName = name;
        CmdSetDisplayName(name);
    }

    [Command]
    void CmdSetDisplayName(string name)
    {
        displayName = name;
        nameLabelRef.GetComponent<Text>().text = name;
        RpcSetDisplayName(name);
    }

    [ClientRpc]
    void RpcSetDisplayName(string name)
    {
        displayName = name;
        nameLabelRef.GetComponent<Text>().text = name;
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

    public bool getShipVisibility()
    {
        return gameObject.GetComponent<SpriteRenderer>().enabled;
    }
}
