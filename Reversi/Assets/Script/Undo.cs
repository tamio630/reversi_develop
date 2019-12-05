using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo : MonoBehaviour {
    private GamePlay gamePlay;

	// Use this for initialization
	void Start () {
        gamePlay = GameObject.Find("gamePlay").GetComponent<GamePlay>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        gamePlay.Undo();
    }
}
