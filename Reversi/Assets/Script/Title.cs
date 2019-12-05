using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour {
    private static int mode = 0;

	

    public static int Mode
    {
        get
        {
            return mode;
        }
        set
        {
            mode = value;
        }
    }
}
