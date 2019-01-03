using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShipCombat : NetworkBehaviour
{

   // public GameObject bulletRef;

    public GameObject healthBarRef;

    public GameObject laserSightRef;

    public GameObject bulletPrefab;

    public HealthBar hBarScriptRef;

    public GameObject explosionPrefab;

    public short maxHP;
    public short currentHP;
    public bool gunnerControlEnabled = false;


    public GameObject currentGunnerRef;
    

	// Use this for initialization
	void Start () {
        currentHP = maxHP;
        hBarScriptRef = healthBarRef.GetComponent<HealthBar>();
        CmdBroadcastDataAllShips();  // update all ships according to server's version 
	}
	
	// Update is called once per frame
	void Update () {

        


        if (gunnerControlEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Debug.Log("Shooting bullet");
                ShootBullet();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("attempting to shoot turret");
                shootTurret();
            }


            if (Input.GetKeyDown(KeyCode.F))
            {
                toggleLaser();
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


    //  From server, broadcasts all data from every ship object
    //  ...so, every ship object for every client will be updated according...
    //  ...to the version on the server
    [Command]
    void CmdBroadcastDataAllShips()
    {

        GameObject[] allShips = GameObject.FindGameObjectsWithTag("PlayerShip");

        //  loop through all ship objects, send data
        for(int i = 0; i < allShips.Length; i++)
        {
            //  broadcast shipHP for this ship
            ShipCombat shipCombatScriptRef = allShips[i].GetComponent<ShipCombat>();
            shipCombatScriptRef.CmdSetHealth(shipCombatScriptRef.currentHP);

            // add more 'paragraphs' for each data value you want to propogate
        }

    }


    void toggleLaser()
    {
        SpriteRenderer laserImageRef = laserSightRef.GetComponent<SpriteRenderer>();
        laserImageRef.enabled = !laserImageRef.enabled;
        
    }

    void ShootBullet()
    {

        currentGunnerRef.GetComponent<ItemSpawner>().shootBullet();
    }

    void shootTurret()
    {
        //  Get angle of shot
        Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float shipAngle = transform.rotation.eulerAngles.z;


        //  relative bearing of shot
        float angleDiff = targetAngle - shipAngle;



        float absDiff = Mathf.Abs(angleDiff);

        if (absDiff > 180)
        {
            absDiff -= 360;
            absDiff = Mathf.Abs(absDiff);

            if (angleDiff > 180)
            {
                angleDiff -= 360;
            }
            if (angleDiff < -180)
            {
                angleDiff += 360;
            }

        }

        //Debug.Log("Target angle: " + targetAngle + ", shipAngle: " + shipAngle + ", angleDiff: " + angleDiff + ", absDiff: " + absDiff);

        if (absDiff < 45 && angleDiff > 0)
        {
            targetAngle = shipAngle + 45;
        }
        else if (absDiff < 45 && angleDiff < 0)
        {
            targetAngle = shipAngle - 45;
        }
        else if (absDiff > 135 && angleDiff > 0)
        {
            targetAngle = shipAngle + 135;
        }
        else if (absDiff > 135 && angleDiff < 0)
        {
            targetAngle = shipAngle - 135;
        }

        currentGunnerRef.GetComponent<ItemSpawner>().shootTurret(targetAngle);

    }

    public void hurtMe(short damage)
    {
        Debug.Log("Damage taken: " + damage);
        CmdAddHealth((short)(-damage));
    }

    ///////////////////////////////////////////////////////////////////////////
    //  Player Explosion networking

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




   

    ///////////////////////////////////////////////////////////////////////////////
    //  COMMANDS

    [Command]
    void CmdSetHealth(short num)
    {
        currentHP = num;
        RpcSetHealth(currentHP);
        RpcUpdateHealthBar(currentHP);
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
