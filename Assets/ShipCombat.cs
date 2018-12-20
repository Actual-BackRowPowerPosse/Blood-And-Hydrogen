using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipCombat : NetworkBehaviour
{

    public GameObject bulletRef;

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
        }
		
	}



    void ShootBullet()
    {
        CmdShootBullet();
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
