using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADselectControl : MonoBehaviour {

    private static int admode = 0;

    public static int ADmode
    {
        get
        {
            return admode;
        }
        set
        {
            admode = value;
        }
    }
}
