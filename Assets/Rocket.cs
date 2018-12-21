using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{

    public int rocketLifeTime; // number of physics updates bullet will last
    public float rocketThrust;
    private Rigidbody2D rbRef;
    private PolygonCollider2D pcRef;
    // Use this for initialization
    void Start()
    {
        rbRef = gameObject.GetComponent<Rigidbody2D>();
        pcRef = gameObject.GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rocketLifeTime--;
        if (rocketLifeTime < 0)
        {
            DestroyObject(gameObject);
        }
        Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        rbRef.AddForce(direction * rocketThrust);

    }
}