using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCreate : MonoBehaviour {
    [SerializeField] private GameObject Base;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                GameObject newtile = GameObject.Instantiate(Base);
                newtile.transform.position = new Vector3(-3.5f+i,-3.5f+j,newtile.transform.position.z);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
