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

    public void UpdateHealthBar(float newHp)
    {
        //  hbar at 1.0
        //  target 0.7
        //  1.0 - 0.7 = 0.3

        //  hbar at 0.8
        //  target 0.3
        //  0.8 - 0.3 = 0.5


        float difference = transform.localScale.x - newHp;
        //Debug.Log("Attempting to update health bar by: " + add);


        if (newHp < 0)
        {
            // set healthbar to 0
            transform.localScale -= new Vector3(transform.localScale.x, 0.0f);
            //Debug.Log("Heath negative, setting to 0");
        }
        else if (newHp > 1.0)
        {
            //  set healthbar to full
            transform.localScale += new Vector3(1.0f - transform.localScale.x, 0.0f);
        }
        else
        {
            //  add healthbar size
            transform.localScale -= new Vector3(difference, 0.0f);
        }
        
    }
}
