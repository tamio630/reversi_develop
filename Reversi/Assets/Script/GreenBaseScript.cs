using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBaseScript : MonoBehaviour {

    public static float COLLISION_SIZE = 1.0f;//ブロックのあたりのサイズ
    private GameObject main_camera = null;//メインカメラ
    private GamePlay gamePlay;

    // Use this for initialization
    void Start () {
        this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        this.gamePlay = GameObject.Find("gamePlay").GetComponent<GamePlay>();

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_position;//マウスの位置
            this.unprojectMousePosition(out mouse_position, Input.mousePosition);//マウスの位置を取得

            //取得したマウス位置をXとYだけにする
            Vector2 mouse_position_xy = new Vector2(mouse_position.x, mouse_position.y);

            if (isContainedPosition(mouse_position_xy))
            {
                if ((Title.Mode == 0 && ADselectControl.ADmode == gamePlay.GetWhich()) || Title.Mode == 1)
                {
                    gamePlay.Put((int)(this.transform.position.x + 5), (int)(this.transform.position.y + 5), gamePlay.GetWhich());
                }
            }
        }
    }

    


    public bool unprojectMousePosition(out Vector3 world_position, Vector3 mouse_position)
    {
        bool ret;

        //板を作成。この板はカメラから見える面が表でブロックの半分のサイズ分、手前に置かれる
        Plane plane = new Plane(Vector3.back, new Vector3(0.0f, 0.0f, -GreenBaseScript.COLLISION_SIZE / 2.0f));

        //カメラとマウスを通る光線を作成
        Ray ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

        float depth;

        //光線（Ray）が板（Plane）に当たっているなら
        if (plane.Raycast(ray, out depth))
        {
            //引数world_positionを、マウスの位置で上書き
            world_position = ray.origin + ray.direction * depth;
            ret = true;
        }
        else//当たっていないなら
        {
            //引数world_positionをゼロのベクターで上書き
            world_position = Vector3.zero;
            ret = false;
        }
        return (ret);
    }

    public bool isContainedPosition(Vector2 position)
    {
        bool ret = false;
        Vector3 center = this.transform.position;
        float h = GreenBaseScript.COLLISION_SIZE / 2.0f;

        do
        {
            //X座標が自分に重なっていないなら、breakでループを抜ける
            if (position.x < center.x - h || center.x + h < position.x)
            {
                break;
            }
            //Y座標が自分に重なっていないなら、breakでループを抜ける
            if (position.y < center.y - h || center.y + h < position.y)
            {
                break;
            }
            //X座標、Y座標の両方が重なっていたら、true（重なっている）を返す
            ret = true;
        } while (false);

        return (ret);
    }
}
