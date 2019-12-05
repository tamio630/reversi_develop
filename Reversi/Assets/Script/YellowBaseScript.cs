using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowBaseScript : MonoBehaviour {

    [SerializeField] private GameObject yellow_base;
    private GameObject[,] yellow_base_list = new GameObject[8, 8];
    private GamePlay gamePlay;

    // Use this for initialization
    void Start () {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject newtile = GameObject.Instantiate(this.yellow_base) as GameObject;
                Vector3 num = newtile.transform.position;
                num = new Vector3(-3.5f, -3.5f, num.z);
                num.x += i;
                num.y += j;
                newtile.transform.position = num;
                yellow_base_list[i, j] = newtile;
                yellow_base_list[i, j].SetActive(false);
            }
        }
        this.gamePlay = GameObject.Find("gamePlay").GetComponent<GamePlay>();
    }
	
	public void PostProcess()
    {
        int[,] copy_stone = gamePlay.Copy_All_Stone();

        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                yellow_base_list[i, j].SetActive(false);
            }
        }

        for (int i = 1; i < 9; i++)
        {
            for(int j = 1; j < 9; j++)
            {
                if (gamePlay.PossibleMass(i, j, gamePlay.GetWhich(), copy_stone))
                {
                    yellow_base_list[i-1, j-1].SetActive(true);
                }
            }
        }
    }
}
