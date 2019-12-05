using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Put_Locate : MonoBehaviour {

	public void Trans_Marker(int i,int j)
    {
        this.transform.position = new Vector3(-3.5f + (i - 1), -3.5f + (j - 1), -1);
    }
}
