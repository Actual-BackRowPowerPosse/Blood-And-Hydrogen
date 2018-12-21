using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public GameObject objRef;
    private Camera camRef;
    private Rigidbody2D rbRef;
    public float camSize;
    public float speedModCoeff;
    public float speedModPow;
    public float speedTranslateCoeff;
    public float speedTranslateActiv;

    public float camMaxTranslateSpeed;
    public float camMaxZoomSpeed;

    private float prevX = 0.0f;
    private float prevY = 0.0f;
    private float prevSize = 0.0f;

    private bool hasSet = false;

    // Use this for initialization
    void Start () {

        camRef = GetComponent<Camera>();
        prevSize = camRef.orthographicSize;
	}

    

	
	// Update is called once per frame
	void Update () {

        
        if(rbRef == null)
        {
            hasSet = false;
        }

        if (hasSet)
        {
            // Debug.Log("Camera has been set");
            //Debug.Log(objRef + "'s current position is: (" + objRef.transform.position.x + ", " + objRef.transform.position.y + ")");


            ////////////////////////////////////////////////////////////////////
            // Enlarging camera based off of speed

            // calculate new size of camera based off of speed
            float newSize = camSize + Mathf.Pow((speedModCoeff * rbRef.velocity.magnitude), speedModPow);

            // prevent camera from 'jumping' -- size will only change by small amount per update
            camRef.orthographicSize += clampFloat(newSize - prevSize, -camMaxZoomSpeed, camMaxZoomSpeed);

            // save current size of camera, so next update will remember
            prevSize = camRef.orthographicSize;

            /////////////////////////////////////////////////////////////////////
            //  Translating camera to player, and adjusting camera forward to show ahead of player

            //  Create vector representing a translation from camera's current position to player
            Vector2 transformVect = new Vector2((rbRef.position.x - transform.position.x), rbRef.position.y - transform.position.y);



            //  Move camera position to show what is ahead of player
            transformVect.x += rbRef.velocity.x * speedTranslateCoeff;
            transformVect.y += rbRef.velocity.y * speedTranslateCoeff / 1.8f; // divided to compensate for short heighth of screen


            // Keep camera from 'jumping' -- will only move at maximum a certain distance each update
            transformVect.x = clampFloat((transform.position.x + transformVect.x) - prevX, -camMaxTranslateSpeed, camMaxTranslateSpeed);
            transformVect.y = clampFloat((transform.position.y + transformVect.y) - prevY, -camMaxTranslateSpeed, camMaxTranslateSpeed);

            //  Actually move camera, pass in vector
            transform.Translate(transformVect);

            //  save current position for next update() call to use
            prevX = transform.position.x;
            prevY = transform.position.y;
        }
        

    }

    public void lookAtObject(GameObject obj)
    {
        Debug.Log("Setting camera to look at " + obj);
        objRef = obj;
        
        rbRef = objRef.GetComponent<Rigidbody2D>();

        hasSet = true;
    }

    

    private float clampFloat(float num, float min, float max)
    {
        float returnVal = num;
        if (num < min)
            returnVal = min;
        else if (num > max)
            returnVal = max;

        return returnVal;
    }

    private float absVal(float num)
    {
        if (num < 0)
            return -num;
        else
            return num;
    }

}
