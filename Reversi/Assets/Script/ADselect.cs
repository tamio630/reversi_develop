using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ADselect : MonoBehaviour {

    public void Onclick()
    {
        if (this.gameObject.CompareTag("Attack"))
        {
            ADselectControl.ADmode = 0;
        }
        else
        {
            ADselectControl.ADmode = 1;
        }
        SceneManager.LoadScene("gamePlay");

    }
}
