using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipCombat : NetworkBehaviour
{

    public GameObject bulletRef;

    public GameObject healthBarRef;
    public HealthBar hBarScriptRef;

    public GameObject explosionPrefab;

    public short maxHP;
    public short currentHP;


    

	// Use this for initialization
	void Start () {
        currentHP = maxHP;
        hBarScriptRef = healthBarRef.GetComponent<HealthBar>();
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
                //hBarScriptRef.UpdateHealthBar(-.2f);
                //healthBarRef.transform.localScale -= new Vector3(0.7f, 0.0f);
            }
        }

        if(currentHP <= 0)
        {
            explode(50);
            Destroy(gameObject);
        }
		
	}

    public void explode(short duration)
    {
        CmdExplode(duration);
    }

    [Command]
    void CmdExplode(short duration)
    {
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.GetComponent<Explosion>().setExplosion(gameObject.transform.position.x, gameObject.transform.position.y, duration);
        RpcExplode(duration);
        Destroy(gameObject);
    }

    [ClientRpc]
    void RpcExplode(short duration)
    {
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.GetComponent<Explosion>().setExplosion(gameObject.transform.position.x, gameObject.transform.position.y, duration);
        Destroy(gameObject);
    }

    void ShootBullet()
    {
        CmdShootBullet();
    }

    public void hurtMe(short damage)
    {
        Debug.Log("Damage taken: " + damage);
        CmdAddHealth((short)(-damage));
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
        //Debug.Log("Adding " + num + " hp to player");
        currentHP += num;
        RpcSetHealth(currentHP);
        RpcUpdateHealthBar(currentHP);
    }


    ////////////////////////////////////////////////////////////////
    //  RPC's

    [ClientRpc]
    public void RpcUpdateHealthBar(float newHP)
    {
        float maxAsFloat = (float)maxHP;
        float HP_Percent = newHP / maxAsFloat;
        hBarScriptRef.UpdateHealthBar(HP_Percent);
    }

    


    [ClientRpc]
    public void RpcSetHealth(short num)
    {
        currentHP = num;
        //healthBarRef.transform.localScale = new Vector3(0.7f, 1.0f);
    }
}
