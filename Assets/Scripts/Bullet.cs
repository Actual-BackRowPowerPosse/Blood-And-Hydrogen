using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

    public int bulletLifeTime; // number of physics updates bullet will last
    public float bulletThrust;
    public short damage;

    private bool layerSet = false;
    bool collision = false;

    public short impactDetonateDelay;

    private Rigidbody2D rbRef;
    public GameObject explosionPrefab;
	// Use this for initialization
	void Start () {
        rbRef = gameObject.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {

        

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        collision = true;
        Debug.Log("Bullet Impact triggered");
        if(other.tag == "PlayerShip" && other.gameObject.layer == 8) //  is a playership in layer 8 (localShips)
        {
            ShipCombat target = other.gameObject.GetComponent<ShipCombat>();
            target.hurtMe(damage);

        }
        //Destroy(gameObject);
        if (hasAuthority)
        {
            CmdSetDelay(impactDetonateDelay);
        }
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<PolygonCollider2D>().enabled = false;

        
        explode(5);

    }

    private void FixedUpdate()
    {
        if (hasAuthority)
        {
            if (!layerSet)
            {
                Debug.Log("Projectile has authority. Won't collide with local ships");
                gameObject.layer = 11; //localProjectiles -- won't collide with playerShip, WILL collide with networkShips
                layerSet = true;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Destroy(gameObject);
            }
        }

        bulletLifeTime--;
        if(bulletLifeTime < 0)
        {
            CmdDetonate();
        }

        //  apply thrust forward -- where bullet is pointed
        float angle = rbRef.rotation;
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
        rbRef.AddForce(dir * bulletThrust);
    }

    private void explode(short duration)
    {
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.GetComponent<Explosion>().setExplosion(gameObject.transform.position.x, gameObject.transform.position.y, duration);
    }

    [Command]
    void CmdSetDelay(short delay)
    {
        bulletLifeTime = delay;

        // no collision registered, and this is a NetworkProjectile -- therefore, is a network-to-network collision
        if (!collision && gameObject.layer == 12) 
        {
            explode(5);
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<PolygonCollider2D>().enabled = false;

        }
        RpcSetDelay(delay);
    }

    [ClientRpc]
    void RpcSetDelay(short delay)
    {
        // no collision registered, and this is a NetworkProjectile -- therefore, is a network-to-network collision
        if (!collision && gameObject.layer == 12)
        {
            explode(5);
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<PolygonCollider2D>().enabled = false;
        }
        bulletLifeTime = delay;
    }

    [Command]
    void CmdDetonate()
    {
        DestroyObject(gameObject);
    }

    [ClientRpc]
    void RpcDetonate()
    {
        DestroyObject(gameObject);
    }
}
