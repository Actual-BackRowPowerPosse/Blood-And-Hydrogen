using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public int bulletLifeTime; // number of physics updates bullet will last
    public float bulletThrust;
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
