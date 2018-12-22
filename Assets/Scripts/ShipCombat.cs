using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ShipCombat : NetworkBehaviour
{

    public GameObject bulletRef;

    public short maxHP;
    public short currentHP;
    public Slider healthBar;

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
                //Debug.Log("Shooting bullet");
                //ShootBullet();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                CmdAddHealth(-20);
            }
        }
		
	}

    float calculateHealth()
    {
        return (currentHP / maxHP);
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

        
        NetworkServer.SpawnWithClientAuthority(bulletObj, connectionToClient);
    }

    [Command]
    void CmdAddHealth(short num)
    {
        currentHP += num;

        //healthBar.value = calculateHealth();
        healthBar.value = 100f;
        Debug.Log("HB Value " + healthBar.value);
        RpcSetHealth(currentHP);
    }


    ////////////////////////////////////////////////////////////////
    //  RPC's

    [ClientRpc]
    void RpcBulletShot()
    {

    }


    [ClientRpc]
    void RpcSetHealth(short num)
    {
        currentHP = num;
        healthBar.value = calculateHealth();

    }
}
