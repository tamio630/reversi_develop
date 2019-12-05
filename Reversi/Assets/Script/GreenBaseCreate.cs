using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBaseCreate : MonoBehaviour {
    [SerializeField] GameObject GreenBase;
    
	// Use this for initialization
	void Start () {
        
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject newtile = GameObject.Instantiate(this.GreenBase) as GameObject;
                Vector3 num = newtile.transform.position;
                num = new Vector3(-3.5f, -3.5f, num.z);
                num.x += i;
                num.y += j;
                newtile.transform.position = num;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        
	}


}
