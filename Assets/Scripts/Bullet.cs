using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

    public int bulletLifeTime; // number of physics updates bullet will last
    public float bulletThrust;
    public short damage;

    private bool layerSet = false;


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
        Debug.Log("Bullet Impact triggered");
        if(other.tag == "PlayerShip" && other.gameObject.layer == 8) //  is a playership in layer 8 (localShips)
        {
            ShipCombat target = other.gameObject.GetComponent<ShipCombat>();
            target.hurtMe(damage);

        }
        //Destroy(gameObject);
        bulletLifeTime = impactDetonateDelay;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<PolygonCollider2D>().enabled = false;

        short explosionDuration = 5;
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.GetComponent<Explosion>().setExplosion(gameObject.transform.position.x, gameObject.transform.position.y, explosionDuration);

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
