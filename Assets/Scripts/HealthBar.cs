using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        

	}

    public void UpdateHealthBar(float add)
    {
        float result = add + transform.localScale.x;
        //Debug.Log("Attempting to update health bar by: " + add);


        if (result < 0)
        {
            // set healthbar to 0
            transform.localScale -= new Vector3(transform.localScale.x, 0.0f);
            //Debug.Log("Heath negative, setting to 0");
        }
        else if (result > 1.0)
        {
            //  set healthbar to full
           // Debug.Log("Health overflow, setting to max");
            transform.localScale += new Vector3(1.0f - transform.localScale.x, 0.0f);
        }
        else
        {
            //  add healthbar size
            transform.localScale += new Vector3(add, 0.0f);
        }
        
    }
}
