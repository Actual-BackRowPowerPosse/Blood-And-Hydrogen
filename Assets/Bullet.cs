using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

    public int bulletLifeTime; // number of physics updates bullet will last
    public float bulletThrust;

    private bool layerSet = false;

    private Rigidbody2D rbRef;
	// Use this for initialization
	void Start () {
        rbRef = gameObject.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    private void FixedUpdate()
    {
        if (hasAuthority)
        {
            gameObject.layer = 8; //localShip -- won't collide with playerShip
            layerSet = true;
        }

        bulletLifeTime--;
        if(bulletLifeTime < 0)
        {
            DestroyObject(gameObject);
        }

        float angle = rbRef.rotation;
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
        rbRef.AddForce(dir * bulletThrust);
    }
}
