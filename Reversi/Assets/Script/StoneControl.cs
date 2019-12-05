using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneControl : MonoBehaviour {
    /*[SerializeField] GameObject BlackStone;
    [SerializeField] GameObject WhiteStone;*/
    [SerializeField] private int rJudge = 2;
    private float reversing = 0f;
    [SerializeField] private float reverseTime = 0.3f;

    // Use this for initialization
    void Start () {
        /*GameObject stone = GameObject.Instantiate(BlackStone);
        stone.transform.position = new Vector3(0.5f, 0.5f, stone.transform.position.z);
        stone = GameObject.Instantiate(BlackStone);
        stone.transform.position = new Vector3(-0.5f, -0.5f, stone.transform.position.z);
        stone = GameObject.Instantiate(WhiteStone);
        stone.transform.position = new Vector3(0.5f, -0.5f, stone.transform.position.z);
        stone = GameObject.Instantiate(WhiteStone);
        stone.transform.position = new Vector3(-0.5f, 0.5f, stone.transform.position.z);*/
    }
	
	// Update is called once per frame
	void Update () {
        if (reversing != 0)
        {
            reversing += Time.deltaTime;
            Reverse();

        }
        if (reversing > reverseTime)
        {
            reversing = 0;
            this.transform.rotation = Quaternion.Euler(rJudge*180f,0f,0.0f); // クォータニオンで回転させる
        }
        if (Input.GetKeyDown(KeyCode.Space)) StartReverse();
    }

    public void Put(int r)//r:白か黒か
    {
        if (r == 0)
        {
            rJudge = 0;
            this.transform.rotation = Quaternion.Euler(rJudge * 180f, 0f, 0.0f); // クォータニオンで回転させる
        }
        else
        {
            rJudge = r;
            this.transform.rotation = Quaternion.Euler(rJudge * 180f, 0f, 0.0f); // クォータニオンで回転させる
        }
    }

    //黒か白か返す
    public int Get()
    {
        return rJudge;
    }

    public bool isReversing()
    {
        if (reversing == 0) return false;
        else return true;
    }

    //リバース中かどうか
    public void StartReverse()
    {
        rJudge = (rJudge + 1) % 2;
        reversing += Time.deltaTime;
    }

    void Reverse()
    {
        Vector3 axis = new Vector3(1f, 0f, 0f); // 回転軸
        float angle = 180f/reverseTime * Time.deltaTime; // 回転の角度
        Quaternion q = Quaternion.AngleAxis(angle, axis); // 軸axisの周りにangle回転させるクォータニオン

        this.transform.rotation = q * this.transform.rotation; // クォータニオンで回転させる

    }
}
