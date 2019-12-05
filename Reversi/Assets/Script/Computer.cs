using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Computer : MonoBehaviour {
    private GamePlay gamePlay;
    public bool ComTurn=false;

    private static double turn_coefficient; //ターン数による係数
    private static int predict=2;//先読みターン数
    private int[,] putMass = new int[10, 10];//現在状況保存用配列
    private int turn;//自分かcomか
    private struct Previous
    {
        int turn;//何ターン進んだ後か
        int which;//どちらがおいたか
        int x;//置いた石の座標(0-8)
        int z;

        public void put(int turn, int which, int x, int z)
        {
            this.turn = turn;
            this.which = which;
            this.x = x;
            this.z = z;
        }

        public int getTurn()
        {
            return turn;
        }

        public int getWhich()
        {
            return which;
        }

        public int getX()
        {
            return x;
        }

        public int getZ()
        {
            return z;
        }

    }
    private Stack<Previous> Undo = new Stack<Previous>();//ひっくり返った石の履歴
    private Stack<Previous> putedStone = new Stack<Previous>();//置いた石の履歴
    private int number_of_turn;//今の総ターン数
    private List<int[]> able_list;
    private double[,] value_list = new double[8, 8];
    [SerializeField] TextAsset textAsset;

    // Use this for initialization
    void Start () {
        gamePlay = GameObject.Find("gamePlay").GetComponent<GamePlay>();
        Previous former = new Previous();
        former.put(0, 0, 0, 0);
        Undo.Push(former);
        putedStone.Push(former);
        string[] lines = textAsset.text.Split('\n');
        for(int i = 0; i < 8; i++)
        {
            string[] line = lines[i].Split();
            for(int j = 0; j < 8; j++)
            {
                value_list[i, j] = double.Parse(line[j]);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (ComTurn==true&&Title.Mode == 0 && gamePlay.GetWhich()!= ADselectControl.ADmode && !gamePlay.Reversing())
        {
            ComTurn = false;
            com();
        }
	}

    void com()
    {
        ////////////////////前処理////////////////////////
        int[] puttingMass = new int[2];//置く場所を保存するための配列　マスの(1-8)番目から指定する
        turn = gamePlay.GetWhich();//computerのターンをゲットする(0 or 1)

        //現在石情報をputMassに保存
        putMass = gamePlay.Copy_All_Stone();

        //現在のターン数
        number_of_turn = gamePlay.GetTurn();

        able_list = gamePlay.PossibleMassList(turn, putMass);

        if (number_of_turn <= 20)
        {
            predict = 2;
            turn_coefficient = 1;
        }
        if (number_of_turn <= 40)
        {
            predict = 2;
            turn_coefficient = 2;
        }
        else if (number_of_turn <= 50)
        {
            predict = 3;
            turn_coefficient = 2.5;
        }
        else
        {
            predict = 10;
            turn_coefficient = 3;
        }

        ////////////////////ここから////////////////////////

        //評価関数を計算し、一番評価の高い場所が[puttingMass]に保存される
        puttingMass=Deep_reading(turn);
        //puttingMass =MaxReviewer(this.turn,putMass);

        ///////////////////ここまで///////////////////////

        //(puttingMass[0],puttingMass[1])の場所に置く処理
        gamePlay.Put(puttingMass[0], puttingMass[1], gamePlay.GetWhich());
    }

    //最大の評価を返す
    double MaxReviewer(int turn,int[,] mass_Status)
    {
        int[,] copyArray = new int[10, 10];
        double max_value;
        copyArray = CopyArray(mass_Status);
        List<int[]> able_list;
        able_list = gamePlay.PossibleMassList(turn, copyArray);
        int[] max_method = { able_list[0][0], able_list[0][1] };

        Reverse(able_list[0][0],able_list[0][1],turn,number_of_turn,copyArray);
        copyArray[able_list[0][0], able_list[0][1]] = turn;
        max_value = Relative_value(copyArray); //相対評価　自分の評価ー相手の評価
        foreach(int[] array in able_list)
        {
            copyArray = CopyArray(mass_Status);
            Reverse(array[0], array[1], turn, number_of_turn, copyArray);
            copyArray[array[0], array[1]] = turn;
            double value =Relative_value(copyArray);
            //Debug.Log(value+":");
            if (max_value < value)
            {
                max_value = value;
                max_method[0] = array[0];
                max_method[1] = array[1];
            }
        }
        //Debug.Log(max_value);

        return max_value;
    }

    //最小の評価を返す
    double MinReviewer(int turn,int[,] mass_Status)
    {
        int[,] copyArray = new int[10, 10];
        double min_value;
        copyArray = CopyArray(mass_Status);
        List<int[]> able_list;
        able_list = gamePlay.PossibleMassList(turn, copyArray);
        int[] min_method = { able_list[0][0], able_list[0][1] };


        Reverse(able_list[0][0], able_list[0][1], turn, number_of_turn, copyArray);
        copyArray[able_list[0][0], able_list[0][1]] = turn;
        min_value = Relative_value(copyArray); //相対評価　自分の評価ー相手の評価
        //Debug.Log(min_value);
        foreach (int[] array in able_list)
        {
            copyArray = CopyArray(mass_Status);
            Reverse(array[0], array[1], turn, number_of_turn, copyArray);
            copyArray[array[0], array[1]] = turn;
            double value = Relative_value(copyArray);
            Debug.Log(value);
            if (min_value > value)
            {
                min_value = value;
                min_method[0] = array[0];
                min_method[1] = array[1];
            }
        }
        Debug.Log("min"+min_value);
        Debug.Log("----------------------------------");
        return min_value;
    }

    //盤面の評価値を返す
    double Review(int[,] array,int turn)
    {
        double sum = 0;
        for(int i = 1; i < 9; i++)
        {
            for(int j = 1; j < 9; j++)
            {
                if (array[i, j] == turn)
                {
                    sum += Reviewer(i, j);
                }
            }
        }

        
        
        double b_sum, a_sum;
        b_sum = sum;
        sum += get_pattern_value0(array, turn);
        a_sum = sum;
        if (a_sum != b_sum)
        {
            Debug.Log("a" + a_sum + "b" + b_sum);
        }

        sum += get_pattern_value1(array, turn);
        //sum += get_pattern_value2(array, turn);
        //sum += get_turn_penalty(array,turn);
        //Debug.Log("penalty" + get_turn_penalty(array,turn));
        //if (get_pattern_value0(array, turn) != 0)
        // Debug.Log("debug"+get_pattern_value0(array, turn));

        return sum;
    }

    //(i,j)に置かれているマスの評価値を返す
    double Reviewer(int i, int j)
    {
        //int reviewResult = 0;
        i--;
        j--;

        return value_list[i,j];
    }

    int[,] CopyArray(int[,] putMass)
    {
        int[,] copyArray = new int[10, 10];
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                copyArray[i, j] = putMass[i, j];
            }
        }
        return copyArray;
    }

    //putMass[]の石をひっくり返す
    public void Reverse(int x, int z, int turn, int nowTurn,int[,] putMass)
    {
        bool flg = false;
        int i, getdata, j;
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
        if (flg)
        {
            i = x - 1;
            while (putMass[i, z] != turn)
            {
                putMass[i, z] = (putMass[i, z] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, i, z);
                Undo.Push(now);
                i--;
            }
        }

        flg = false;
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
        if (flg)
        {
            i = x + 1;
            while (putMass[i, z] != turn)
            {
                putMass[i, z] = (putMass[i, z] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, i, z);
                Undo.Push(now);
                i++;
            }
        }

        flg = false;
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
        if (flg)
        {
            i = z + 1;
            while (putMass[x, i] != turn)
            {
                putMass[x, i] = (putMass[x, i] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, x, i);
                Undo.Push(now);
                i++;
            }
        }

        flg = false;
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
        if (flg)
        {
            i = z - 1;
            while (putMass[x, i] != turn)
            {
                putMass[x, i] = (putMass[x, i] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, x, i);
                Undo.Push(now);
                i--;
            }
        }

        flg = false;
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
        if (flg)
        {
            i = x + 1;
            j = z + 1;
            while (putMass[i, j] != turn)
            {
                putMass[i, j] = (putMass[i, j] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, i, j);
                Undo.Push(now);
                i++;
                j++;
            }
        }

        flg = false;
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
        if (flg)
        {
            i = x - 1;
            j = z + 1;
            while (putMass[i, j] != turn)
            {
                putMass[i, j] = (putMass[i, j] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, i, j);
                Undo.Push(now);
                i--;
                j++;
            }
        }

        flg = false;
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
        if (flg)
        {
            i = x + 1;
            j = z - 1;
            while (putMass[i, j] != turn)
            {
                putMass[i, j] = (putMass[i, j] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, i, j);
                Undo.Push(now);
                i++;
                j--;
            }
        }

        flg = false;
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
        if (flg)
        {
            i = x - 1;
            j = z - 1;
            while (putMass[i, j] != turn)
            {
                putMass[i, j] = (putMass[i, j] + 1) % 2;
                Previous now = new Previous();
                now.put(nowTurn, turn, i, j);
                Undo.Push(now);
                i--;
                j--;
            }
        }
    }

    private int Get_enemy_turn(int turn)
    {
        int enemy_turn;
        enemy_turn = (turn+1) % 2;
        return enemy_turn;
    }

    private double get_pattern_value0(int[,] array,int turn)
    {
        double penalty = -1.5;
        if ((array[1, 1] == turn) && (array[1, 3] == turn))
        {
            if (array[1, 2] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }
        else if ((array[1, 1] == turn) && (array[3, 1] == turn))
        {
            if (array[2, 1] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }
        else if ((array[1, 6] == turn) && (array[1, 8] == turn))
        {
            if (array[1, 7] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }
        else if ((array[3, 8] == turn) && (array[1, 8] == turn))
        {
            if (array[2, 8] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }
        else if ((array[6, 1] == turn) && (array[8, 1] == turn))
        {
            if (array[7, 1] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }
        else if ((array[8, 1] == turn) && (array[8, 3] == turn))
        {
            if (array[8, 2] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }
        else if ((array[8, 6] == turn) && (array[8, 8] == turn))
        {
            if (array[8, 7] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }
        else if ((array[6, 8] == turn) && (array[8, 8] == turn))
        {
            if (array[7, 8] == turn)
            {
                return (-1 * penalty * turn_coefficient);
            }
            else
            {
                Debug.Log("Penalty");
                return penalty * turn_coefficient;
            }
        }

            return 0;
    }

    private double get_pattern_value1(int [,] array,int turn)
    {
        double penalty = -2.5;
        if((array[1,2]==Get_enemy_turn(turn))&&(array[1,3]==Get_enemy_turn(turn))&&(array[1,4]==Get_enemy_turn(turn))&&(array[1,5]==Get_enemy_turn(turn))&&(array[1,6]==Get_enemy_turn(turn))&&array[1,7]==Get_enemy_turn(turn))
        {
            if ((array[1, 1] == Get_enemy_turn(turn)) && (array[1, 8] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 1] == turn) && (array[1, 8] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 1] == Get_enemy_turn(turn)) && (array[1, 8] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 1] == turn) && (array[1, 8] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
        }else if ((array[2, 1] == Get_enemy_turn(turn)) && (array[3, 1] == Get_enemy_turn(turn)) && (array[4, 1] == Get_enemy_turn(turn)) && (array[5, 1] == Get_enemy_turn(turn)) && (array[6, 1] == Get_enemy_turn(turn)) && array[7, 1] == Get_enemy_turn(turn))
        {
            if ((array[1, 1] == Get_enemy_turn(turn)) && (array[8, 1] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 1] == turn) && (array[8, 1] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 1] == Get_enemy_turn(turn)) && (array[8, 1] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 1] == turn) && (array[8, 1] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
        }else if ((array[8, 2] == Get_enemy_turn(turn)) && (array[8, 3] == Get_enemy_turn(turn)) && (array[8, 4] == Get_enemy_turn(turn)) && (array[8, 5] == Get_enemy_turn(turn)) && (array[8, 6] == Get_enemy_turn(turn)) && array[8, 7] == Get_enemy_turn(turn))
        {
            if ((array[8, 1] == Get_enemy_turn(turn)) && (array[8, 8] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[8, 1] == turn) && (array[8, 8] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[8, 1] == Get_enemy_turn(turn)) && (array[8, 8] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[8, 1] == turn) && (array[8, 8] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
        }
        else if ((array[2, 8] == Get_enemy_turn(turn)) && (array[3, 8] == Get_enemy_turn(turn)) && (array[4, 8] == Get_enemy_turn(turn)) && (array[5, 8] == Get_enemy_turn(turn)) && (array[6, 8] == Get_enemy_turn(turn)) && (array[7, 8] == Get_enemy_turn(turn)))
        {
            if ((array[1, 8] == Get_enemy_turn(turn)) && (array[8, 8] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 8] == turn) && (array[8, 8] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 8] == Get_enemy_turn(turn)) && (array[8, 8] == turn))
            {
                return (penalty * turn_coefficient);
            }
            else if ((array[1, 8] == turn) && (array[8, 8] == Get_enemy_turn(turn)))
            {
                return (penalty * turn_coefficient);
            }
        }
        return 0;
    }

    private double get_pattern_value2(int [,] array,int turn)
    {
        if ((array[4, 4] == Get_enemy_turn(turn)) && (array[4, 5] == Get_enemy_turn(turn)) && (array[4, 6] == Get_enemy_turn(turn)) && (array[5, 4] == turn) && (array[5, 5] == turn) && (array[5, 6] == turn)){
            return -0.5;
        }else if((array[4, 4] == turn) && (array[4, 5] == turn) && (array[4, 6] == turn) && (array[5, 4] == Get_enemy_turn(turn)) && (array[5, 5] == Get_enemy_turn(turn)) && (array[5, 6] == Get_enemy_turn(turn))){
            return -0.5;
        }
        return 0;
    }

    private double get_turn_penalty(int[,] array,int turn)
    {

        return (get_stone_count(array, turn) - get_stone_count(array, Get_enemy_turn(turn))) * (number_of_turn * 0.02) - 0.05;
    }

    private int get_stone_count(int[,] array,int turn)
    {
        int sum = 0;
        for(int i = 1; i < 9; i++)
        {
            for(int j = 1; j < 9; j++)
            {
                if (array[i, j] == turn)
                {
                    sum += 1;
                }
            }
        }
        return sum;
    }

    private double Relative_value(int [,] copyArray)
    {
        double max_value;
        max_value = Review(copyArray, turn) - Review(copyArray, Get_enemy_turn(turn));
        return max_value;
    }

    int[] Deep_reading(int turn)
    {
        int[,] copyArray = new int[10, 10];
        double max_value;
        copyArray = CopyArray(putMass);
        List<int[]> able_list;
        able_list = gamePlay.PossibleMassList(turn, putMass);
        int[] max_method = { able_list[0][0], able_list[0][1] };

        Reverse(able_list[0][0], able_list[0][1], turn, number_of_turn, copyArray);
        copyArray[able_list[0][0], able_list[0][1]] = turn;
        max_value = Deep_reading2(copyArray, Get_enemy_turn(turn),1); //相対評価　自分の評価ー相手の評価
        foreach (int[] array in able_list)
        {
            copyArray = CopyArray(putMass);
            Reverse(array[0], array[1], turn, number_of_turn, copyArray);
            copyArray[array[0], array[1]] = turn;
            double value = Deep_reading2(copyArray,Get_enemy_turn(turn),1);
            //Debug.Log(value + ":");
            if (max_value < value)
            {
                //Debug.Log("max" + max_value + "value" + value);
                max_value = value;
                max_method[0] = array[0];
                max_method[1] = array[1];
            }
        }
        Debug.Log("max"+max_value);

        return max_method;
    }

    private double Deep_reading2(int[,] mass_Status,int turn,int n)
    {
        List<int[]> able_list;
        able_list= gamePlay.PossibleMassList(turn, mass_Status);
        double cut_value;
        if (able_list.Count == 0)
        {
            turn = Get_enemy_turn(turn);
            able_list= gamePlay.PossibleMassList(turn, mass_Status);
            //Debug.Log("passed");
            if (able_list.Count == 0)
            {
                return (get_stone_count(mass_Status, this.turn) - get_stone_count(mass_Status, Get_enemy_turn(this.turn)))*100.0;
            }
        }

        int[,] copyArray = new int[10, 10];
        double max_value;
        double min_value;
        copyArray = CopyArray(mass_Status);
        int[] max_method = { able_list[0][0], able_list[0][1] };
        int[] min_method = { able_list[0][0], able_list[0][1] };

        Reverse(able_list[0][0], able_list[0][1], turn, number_of_turn, copyArray);
        copyArray[able_list[0][0], able_list[0][1]] = turn;

        if (n != predict)
        {
            max_value = Deep_reading2(copyArray, Get_enemy_turn(turn), n + 1); //相対評価　自分の評価ー相手の評価
            min_value = max_value;

            foreach (int[] array in able_list)
            {
                copyArray = CopyArray(mass_Status);
                Reverse(array[0], array[1], turn, number_of_turn, copyArray);
                copyArray[array[0], array[1]] = turn;

                /*if (turn == this.turn)
                {
                    if (MaxReviewer(turn, mass_Status) > 5)
                    {
                        return 5;
                    }
                }
                else
                {
                    if(MinReviewer(turn, mass_Status) < -5)
                    {
                        return -5;
                    }
                }*/

                double value = Deep_reading2(copyArray, Get_enemy_turn(turn), n + 1);
                //Debug.Log(value + ":");
                if (max_value < value)
                {
                    //Debug.Log("max" + max_value + "value" + value);
                    max_value = value;
                    max_method[0] = array[0];
                    max_method[1] = array[1];
                    /*cut_value = max_cut();
                    if (max_value > cut_value && turn==this.turn)
                        return cut_value;*/
                }

                if (min_value > value && turn!=this.turn)
                {
                    min_value = value;
                    min_method[0] = array[0];
                    min_method[1] = array[1];

                    /*cut_value = min_cut();
                    if (min_value < cut_value)
                        return cut_value;*/
                }


            }
        }
        else
        {
            if (turn == this.turn)
            {
                return MaxReviewer(turn, mass_Status);
            }
            return MinReviewer(turn, mass_Status);
        }

        if (turn == this.turn)
        {
            return max_value;
        }
        return min_value;

    }

    private double min_cut()
    {
        if (number_of_turn <= 40)
        {
            return -2;
        }
        else
        {
            return -5;
        }
    }

    private double max_cut()
    {
        if (number_of_turn <= 40)
        {
            return 2;
        }
        else {
            return 5;
        }
    }

}
