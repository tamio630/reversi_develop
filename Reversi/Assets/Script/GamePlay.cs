using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour {
    [SerializeField] private GameObject readStone;
    private GameObject[,] stone = new GameObject[10, 10];
    private StoneControl[,] putMass = new StoneControl[10, 10];
    private int which;
    private int turnCount;
    public int Black; //gamePlay.Black
    public int White;
    private bool end;
    private Text text;
    private Text blackText;
    private Text whiteText;
    private Stack<Previous> undo=new Stack<Previous>();
    private Stack<Previous> putedStone = new Stack<Previous>();
    [SerializeField] private Computer com;
    private int[,] copyMass = new int[10, 10];
    private YellowBaseScript put_able;
    [SerializeField] private GameObject marker;

    private struct Previous
    {
        int turn;
        int which;
        int x;
        int y;

        public void Put(int turn, int which, int x, int y)
        {
            this.turn = turn;
            this.which = which;
            this.x = x;
            this.y = y;
        }

        public int GetTurn()
        {
            return turn;
        }

        public int GetWhich()
        {
            return which;
        }

        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }
    }

    // Use this for initialization
    void Start () {
        which = 0;
        end = false;
        text = GameObject.Find("lastText").GetComponent<Text>();
        blackText = GameObject.Find("Black").GetComponent<Text>();
        whiteText = GameObject.Find("White").GetComponent<Text>();
        turnCount = 1;
        Black = 2;
        White = 2;
        blackText.text = "Black:"+Black;
        whiteText.text = "White:"+White;
        marker=GameObject.Instantiate(this.marker) as GameObject;
        marker.SetActive(false);

        Debug.Log("mode:" + Title.Mode+" admode:"+ADselectControl.ADmode);
        for(int i = 0; i < 10; i++)
        {
            //石の敷き詰め
            for(int j = 0; j < 10; j++)
            {
                stone[i, j] = GameObject.Instantiate(readStone) as GameObject;
                stone[i,j].transform.position = new Vector3(-4.5f + i, -4.5f + j,-0.252f);
                putMass[i, j] = stone[i, j].GetComponent<StoneControl>();
                stone[i, j].SetActive(false);
            }

        }

        //始め4つ配置
        stone[4, 4].SetActive(true);
        stone[4, 5].SetActive(true);
        stone[5, 4].SetActive(true);
        stone[5, 5].SetActive(true);


        putMass[4, 4].Put(0);
        putMass[4, 5].Put(1);
        putMass[5, 4].Put(1);
        putMass[5, 5].Put(0);
        
        /*
        stone[1, 1].SetActive(true);
        stone[1, 2].SetActive(true);
        stone[1, 3].SetActive(true);

        putMass[1, 1].Put(0);
        putMass[1, 2].Put(1);
        putMass[1, 3].Put(0);
        */

        /*デバッグ用
        stone[2, 8].SetActive(true);
        stone[3, 8].SetActive(true);
        stone[4, 8].SetActive(true);
        stone[5, 8].SetActive(true);
        stone[6, 8].SetActive(true);
        stone[7, 8].SetActive(true);

        putMass[2, 8].Put(0);
        putMass[3, 8].Put(0);
        putMass[4, 8].Put(0);
        putMass[5, 8].Put(0);
        putMass[6, 8].Put(1);
        putMass[7, 8].Put(0);
        デバッグ終わり*/
        


        Previous former = new Previous();
        former.Put(0, 0, 0, 0);
        undo.Push(former);
        putedStone.Push(former);

        if (Title.Mode == 0 && GetWhich() != ADselectControl.ADmode)
        {
            com.ComTurn = true;
        }

        put_able = this.GetComponent<YellowBaseScript>();
        put_able.PostProcess();

    }

    // Update is called once per frame
    /*void Update () {
        if (end == true)
        {
            if (!Reversing())
            {

                if (Black > White)
                {
                    text.text = "Black Win";
                }
                else if(Black<White)
                {
                    text.text = "White Win";
                }
                else
                {
                    text.text = "Draw";
                }
            }
        }
	}*/

    public int CopyMass(int x,int y)
    {
        return putMass[x, y].Get();
    }

    public int[,] Copy_All_Stone()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                copyMass[i, j] = CopyMass(i, j);
            }
        }

        return copyMass;
    }

    public bool Reversing()
    {
        for(int i=1; i < 9; i++)
        {
            for(int j = 1; j < 9; j++)
            {
                if (putMass[i,j].isReversing() == true) return true;
            }
        }
        return false;
    }

    public void Put(int x,int y,int turn)
    {

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                copyMass[i, j] = CopyMass(i, j);
            }
        }

        if (PossibleMass(x, y, turn, copyMass))
        {
            stone[x, y].SetActive(true);
            putMass[x, y].Put(turn);
            Previous p = new Previous();
            p.Put(turnCount, which,x, y);
            putedStone.Push(p);
            Reverse(x, y, turn, turnCount);
            which = (which + 1) % 2;
            turnCount++;

            if(end = endJudge())
            {
                bwCalc();
                string a="black win";
                if (Black < White) a = "white win";
                if (Black == White) a = "dwaw";
                blackText.text = "Black" + Black;
                whiteText.text = "White:" + White+"\n\n終了！ "+a;
                return;
            }

            if(Title.Mode == 0 && GetWhich() != ADselectControl.ADmode)
            {
                com.ComTurn = true;
            }

            put_able.PostProcess();
            marker.SetActive(true);
            marker.GetComponent<Put_Locate>().Trans_Marker(x, y);
        }
        bwCalc();
        blackText.text = "Black"+Black;
        whiteText.text = "White:"+White;
    }

    void bwCalc()
    {
        Black = 0;
        White = 0;
        for (int i = 1; i < 9; i++)
        {
            for (int j = 1; j < 9; j++)
            {
                if (putMass[i, j].Get() == 0)
                {
                    Black++;
                }
                else if (putMass[i, j].Get() == 1)
                {
                    White++;
                }
            }
        }
    }

    bool endJudge()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                copyMass[i, j] = CopyMass(i, j);
            }
        }

        for (int i = 1; i < 9; i++)
        {
            for (int j = 1; j < 9; j++)
            {
                if (PossibleMass(i, j, which, copyMass)) return false;
            }
        }

        which = (which + 1) % 2;

        for (int i = 1; i < 9; i++)
        {
            for (int j = 1; j < 9; j++)
            {
                if (PossibleMass(i, j, which, copyMass)) return false;
            }
        }

        return true;
    }

    public List<int[]> PossibleMassList(int turn,int[,] putMass)
    {
        List<int[]> li = new List<int[]>();
        for(int i = 1; i < 9; i++)
        {
            for(int j = 1; j < 9; j++)
            {
                if (PossibleMass(i, j,turn,putMass))
                {
                    int[] a = { i, j };
                    li.Add(a);
                }
            }
        }
        return li;
    }

    //指定したマスがおけるかどうか判断し、bool型で返す
    public bool PossibleMass(int x, int z, int turn, int[,] putMass)
    {//turnは今どっちのターンか、putMassは判断する10*10
        bool flg = false;
        int i, getdata, j;
        if (putMass[x, z] == 2)
        {
            //左方向
            if (putMass[x - 1, z] == (turn + 1) % 2)
            {
                i = x - 2;
                while ((getdata = putMass[i, z]) != 2)
                {
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i--;
                }
            }
            //右方向
            if (putMass[x + 1, z] == (turn + 1) % 2)
            {
                i = x + 2;
                while ((getdata = putMass[i, z]) != 2)
                {
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i++;
                }
            }
            //上方向
            if (putMass[x, z + 1] == (turn + 1) % 2)
            {
                i = z + 2;
                while ((getdata = putMass[x, i]) != 2)
                {   //上
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i++;
                }
            }
            //下方向
            if (putMass[x, z - 1] == (turn + 1) % 2)
            {
                i = z - 2;
                while ((getdata = putMass[x, i]) != 2)
                {
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i--;
                }
            }
            //斜め右上
            if (putMass[x + 1, z + 1] == (turn + 1) % 2)
            {
                i = x + 2;
                j = z + 2;
                while ((getdata = putMass[i, j]) != 2)
                {
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i++;
                    j++;
                }
            }
            //斜め左上
            if (putMass[x - 1, z + 1] == (turn + 1) % 2)
            {
                i = x - 2;
                j = z + 2;
                while ((getdata = putMass[i, j]) != 2)
                {
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i--;
                    j++;
                }
            }
            //斜め右下
            if (putMass[x + 1, z - 1] == (turn + 1) % 2)
            {
                i = x + 2;
                j = z - 2;
                while ((getdata = putMass[i, j]) != 2)
                {
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i++;
                    j--;
                }
            }
            //斜め左下
            if (putMass[x - 1, z - 1] == (turn + 1) % 2)
            {
                i = x - 2;
                j = z - 2;
                while ((getdata = putMass[i, j]) != 2)
                {
                    if (getdata == turn)
                    {
                        flg = true;
                    }
                    i--;
                    j--;
                }
            }
        }
        return flg;
    }

    public void Undo()
    {
        Previous p1,p2;
        if (!end&&turnCount!=1)
        {
            if (Title.Mode == 1)
            {
                p1 = putedStone.Pop();
                putMass[p1.GetX(), p1.GetY()].Put(2);
                
                while(undo.Peek().GetTurn() == p1.GetTurn())
                {
                    p2 = undo.Pop();
                    putMass[p2.GetX(), p2.GetY()].Put((p2.GetWhich()+1)%2);
                }
                stone[p1.GetX(), p1.GetY()].SetActive(false);
                turnCount--;
                which = p1.GetWhich();
                put_able.PostProcess();
                if (turnCount == 1)
                    marker.SetActive(false);
                else
                    marker.GetComponent<Put_Locate>().Trans_Marker(putedStone.Peek().GetX(), putedStone.Peek().GetY());
            }
            else
            {   if (!(GetTurn() == 2 && GetWhich() == ADselectControl.ADmode))
                {
                    while (undo.Peek().GetTurn() != 0 && undo.Peek().GetWhich() != ADselectControl.ADmode)
                    {
                        p1 = putedStone.Pop();
                        putMass[p1.GetX(), p1.GetY()].Put(2);

                        while (undo.Peek().GetTurn() == p1.GetTurn())
                        {
                            p2 = undo.Pop();
                            putMass[p2.GetX(), p2.GetY()].Put((p2.GetWhich() + 1) % 2);
                        }
                        stone[p1.GetX(), p1.GetY()].SetActive(false);
                        turnCount--;
                        which = p1.GetWhich();
                    }
                    p1 = putedStone.Pop();
                    putMass[p1.GetX(), p1.GetY()].Put(2);

                    while (undo.Peek().GetTurn() == p1.GetTurn())
                    {
                        p2 = undo.Pop();
                        putMass[p2.GetX(), p2.GetY()].Put((p2.GetWhich() + 1) % 2);
                    }
                    stone[p1.GetX(), p1.GetY()].SetActive(false);
                    turnCount--;
                    which = p1.GetWhich();
                    put_able.PostProcess();
                    if (turnCount == 1)
                        marker.SetActive(false);
                    else
                        marker.GetComponent<Put_Locate>().Trans_Marker(putedStone.Peek().GetX(), putedStone.Peek().GetY());
                }

            }
        }

        bwCalc();
        blackText.text = "Black:"+Black;
        whiteText.text = "White:"+White;

    }

    public int Get(int i, int j)
    {
        return putMass[i, j].Get();
    }

    public int GetWhich()
    {
        return which;
    }

    public int GetTurn()
    {
        return turnCount;
    }

    public void Reverse(int x, int y, int turn, int nowTurn)
    {
        bool flg = false;
        int i, getdata, j;
        //左方向
        if (Get(x - 1, y) == (GetWhich() + 1) % 2)
        {
            i = x - 2;
            while ((getdata = Get(i, y)) != 2)
            {
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i--;
            }
        }
        if (flg)
        {
            i = x - 1;
            while (Get(i, y) != GetWhich())
            {
                Previous now = new Previous();
                now.Put(nowTurn, turn, i, y);
                undo.Push(now);
                putMass[i, y].StartReverse();
                i--;
            }
        }

        flg = false;
        //右方向
        if (Get(x + 1, y) == (GetWhich() + 1) % 2)
        {
            i = x + 2;
            while ((getdata = Get(i, y)) != 2)
            {
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i++;
            }
        }
        if (flg)
        {
            i = x + 1;
            while (Get(i, y) != GetWhich())
            {
                putMass[i, y].StartReverse();
                Previous now = new Previous();
                now.Put(nowTurn, turn, i, y);
                undo.Push(now);
                i++;
            }
        }

        flg = false;
        //上方向
        if (Get(x, y + 1) == (GetWhich() + 1) % 2)
        {
            i = y + 2;
            while ((getdata = Get(x, i)) != 2)
            {   //上
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i++;
            }
        }
        if (flg)
        {
            i = y + 1;
            while (Get(x, i) != GetWhich())
            {
                putMass[x, i].StartReverse();
                Previous now = new Previous();
                now.Put(nowTurn, turn, x, i);
                undo.Push(now);
                i++;
            }
        }

        flg = false;
        //下方向
        if (Get(x, y - 1) == (GetWhich() + 1) % 2)
        {
            i = y - 2;
            while ((getdata = Get(x, i)) != 2)
            {
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i--;
            }
        }
        if (flg)
        {
            i = y - 1;
            while (Get(x, i) != GetWhich())
            {
                putMass[x, i].StartReverse();
                Previous now = new Previous();
                now.Put(nowTurn, turn, x, i);
                undo.Push(now);
                i--;
            }
        }

        flg = false;
        //斜め右上
        if (Get(x + 1, y + 1) == (GetWhich() + 1) % 2)
        {
            i = x + 2;
            j = y + 2;
            while ((getdata = Get(i, j)) != 2)
            {
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i++;
                j++;
            }
        }
        if (flg)
        {
            i = x + 1;
            j = y + 1;
            while (Get(i, j) != GetWhich())
            {
                putMass[i, j].StartReverse();
                Previous now = new Previous();
                now.Put(nowTurn, turn, i, j);
                undo.Push(now);
                i++;
                j++;
            }
        }

        flg = false;
        //斜め左上
        if (Get(x - 1, y + 1) == (GetWhich() + 1) % 2)
        {
            i = x - 2;
            j = y + 2;
            while ((getdata = Get(i, j)) != 2)
            {
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i--;
                j++;
            }
        }
        if (flg)
        {
            i = x - 1;
            j = y + 1;
            while (Get(i, j) != GetWhich())
            {
                putMass[i, j].StartReverse();
                Previous now = new Previous();
                now.Put(nowTurn, turn, i, j);
                undo.Push(now);
                i--;
                j++;
            }
        }

        flg = false;
        //斜め右下
        if (Get(x + 1, y - 1) == (GetWhich() + 1) % 2)
        {
            i = x + 2;
            j = y - 2;
            while ((getdata = Get(i, j)) != 2)
            {
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i++;
                j--;
            }
        }
        if (flg)
        {
            i = x + 1;
            j = y - 1;
            while (Get(i, j) != GetWhich())
            {
                putMass[i, j].StartReverse();
                Previous now = new Previous();
                now.Put(nowTurn, turn, i, j);
                undo.Push(now);
                i++;
                j--;
            }
        }

        flg = false;
        //斜め左下
        if (Get(x - 1, y - 1) == (GetWhich() + 1) % 2)
        {
            i = x - 2;
            j = y - 2;
            while ((getdata = Get(i, j)) != 2)
            {
                if (getdata == GetWhich())
                {
                    flg = true;
                }
                i--;
                j--;
            }
        }
        if (flg)
        {
            i = x - 1;
            j = y - 1;
            while (Get(i, j) != GetWhich())
            {
                putMass[i, j].StartReverse();
                Previous now = new Previous();
                now.Put(nowTurn, turn, i, j);
                undo.Push(now);
                i--;
                j--;
            }
        }
    }

}
