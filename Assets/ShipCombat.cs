using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipCombat : NetworkBehaviour
{
    public GameObject bulletRef;
    public GameObject rocketRef;
    
    public short maxHP;
    public short currentHP;

	// Use this for initialization
	void Start () {
        currentHP = maxHP;
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

            if (Input.GetKeyDown(KeyCode.T))
            {
                CmdAddHealth(-20);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Shooting rocket");
                ShootRocket();
            }
        }
		
	}



    void ShootBullet()
    {
        CmdShootBullet();
    }

    void ShootRocket()
    {
        CmdShootRocket();
    }


    ///////////////////////////////////////////////////////////////////////////////
    //  COMMANDS

    [Command]
    void CmdShootBullet()
    {
        GameObject bulletObj = Instantiate(bulletRef);
        bulletObj.transform.position = gameObject.transform.position; // set object to same position as 'THIS' object
        bulletObj.transform.rotation = gameObject.transform.rotation;
        bulletObj.GetComponent<Rigidbody2D>().velocity = gameObject.GetComponent<Rigidbody2D>().velocity; // add ship's initial velocity to bullet

        Physics2D.IgnoreCollision(bulletObj.GetComponent<PolygonCollider2D>(), gameObject.GetComponent<PolygonCollider2D>());
        NetworkServer.Spawn(bulletObj);
    }
    [Command]
    void CmdShootRocket()
    {
        GameObject rocketObj = Instantiate(rocketRef);
        rocketObj.transform.position = gameObject.transform.position; // set object to same position as 'THIS' object
        rocketObj.transform.rotation = gameObject.transform.rotation;
        rocketObj.GetComponent<Rigidbody2D>().velocity = gameObject.GetComponent<Rigidbody2D>().velocity; // add ship's initial velocity to rocket

        Physics2D.IgnoreCollision(rocketObj.GetComponent<PolygonCollider2D>(), gameObject.GetComponent<PolygonCollider2D>());
        NetworkServer.Spawn(rocketObj);
    }

    [Command]
    void CmdAddHealth(short num)
    {
        currentHP += num;
        RpcSetHealth(currentHP);
    }


    [ClientRpc]
    void RpcSetHealth(short num)
    {
        currentHP = num;

    }
}
