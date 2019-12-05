using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSelect : MonoBehaviour {
    

    public void Onclick()
    {
        if (this.gameObject.CompareTag("CPU"))
        {
            Title.Mode = 0;
            SceneManager.LoadScene("ADselect");
        }
        else
        {
            Title.Mode = 1;
            SceneManager.LoadScene("gamePlay");
        }
        
    }
}
