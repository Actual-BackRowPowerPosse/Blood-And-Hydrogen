using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{


    public short timer;
    public short timeLeft;
    bool startTick;
    private Vector3 growBy;

    // Use this for initialization
    void Start()
    {
        //Debug.Log("Explosion created");
        
    }

    // Update is called once per frame
    void Update()
    {

    }


    void FixedUpdate()
    {
        if (startTick)
        {
            timeLeft--;
            transform.localScale += growBy;
            if(timeLeft < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void setExplosion(float x, float y, short duration)
    {
        transform.position = new Vector2(x, y);
        timer = duration;
        timeLeft = timer;
        startTick = true;
        growBy = new Vector3(transform.localScale.x / timer, transform.localScale.y / timer);
        transform.localScale -= transform.localScale; //shrink to nothing

        // set random rotation
        Vector3 euler = transform.eulerAngles;
        euler.z = Random.Range(0f, 360f);
        transform.eulerAngles = euler;
    }
}
