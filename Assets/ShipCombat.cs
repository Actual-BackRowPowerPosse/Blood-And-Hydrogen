using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipCombat : NetworkBehaviour
{

    public GameObject bulletRef;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Shooting bullet");
                ShootBullet();
            }
        }
		
	}

    void ShootBullet()
    {
        CmdShootBullet();
    }

    [Command]
    void CmdShootBullet()
    {
        GameObject bulletObj = Instantiate(bulletRef);
        bulletObj.transform.position = gameObject.transform.position; // set object to same position as 'THIS' object
        bulletObj.transform.rotation = gameObject.transform.rotation;
        bulletObj.GetComponent<Rigidbody2D>().velocity = gameObject.GetComponent<Rigidbody2D>().velocity; // add ship's initial velocity to bullet

        NetworkServer.Spawn(bulletObj);
    }
}
