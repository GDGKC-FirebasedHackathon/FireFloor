using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class GameManager : MonoBehaviour {

    public RectTransform dump;
    public RectTransform hand;

    public RectTransform Player2;
    public RectTransform Player3;
    public RectTransform Player4;
    public RectTransform Player5;
    public RectTransform Player6;

    public int MyNumber; // 내 번호
    public int MaxPlayer; // 게임 방에 총 인원

    public byte[] RecvData = new byte[1024]; //받는 데이터
    public int RecvData_Length;

    public byte[] MyCardList_RecvData = new byte[1024];   // 5

    public int MyCardList_Length; 
    public byte[] MyCardList // 내 카드의 배열 
    {
        get
        {
            byte[] Temp = new byte[MyCardList_Length-1];
            for(int i=0,k=1; i<Temp.Length;i++,k++)
            {
                Temp[i] = MyCardList_RecvData[k];
            }
            return Temp;
        }
    }

    public byte DumpCard = 255; // Dump 카드

    // Question
    // 채워야 할 부분--------
    public byte QuestionTarget;
    public byte QuestionCardName;
    public byte QuestionQ; //2면 킹
    public bool OK_Question = false; // 준비되면 True
    // ----------------------
    public byte[] QuestionData
    {
        get
        {
            byte[] Temp = new byte[4]; // Type , Target , CardName , Q
            Temp[0] = (byte)1;
            Temp[1] = QuestionTarget;
            Temp[2] = QuestionCardName;
            Temp[3] = QuestionQ;

            return Temp;
        }
    }

    public byte[] RecvData_Question = new byte[1024];
    public int RecvData_Question_Length;
    public byte QuestionTarget_R; // 만약 자기를 지칭하면 따로 처리해야 함.
    public byte QuestionCardName_R;
    public byte QuestionQ_R;
    public byte[] Question
    {
        get
        {
            byte[] Temp = new byte[RecvData_Question_Length];
            QuestionTarget_R = Temp[1];
            QuestionCardName_R = Temp[2];
            QuestionQ_R = Temp[3];

            return Temp;
        }
    }

    public byte AnswerTarget_R;
    public byte AnswerA_R;

    // Answer
    public byte AnswerTarget
    {
        get
        {
            return QuestionTarget_R;
        }
    }
    // 채워야 할 부분 -----
    public byte AnswerA;
    // --------------------
    public byte[] AnswerData
    {
        get
        {
            byte[] Temp = new byte[3];
            Temp[0] = 2;
            Temp[1] = AnswerTarget;
            Temp[2] = AnswerA;

            return Temp;
        }
    }

    public byte[] RecvData_GraveCard = new byte[1024];
    public int RecvData_GraveCard_Length;
    public byte[][] GraveCard = new byte[6][]; // 앞에 있는 []가 유저, 뒤에 있는 값이 무덤 카드 갯수 + 종류

    public byte[] PlayerCards = new byte[6]; // 앞에 있는 []가 유저, 뒤에 있는 값이 카드 갯수

    public byte[] PassLock = new byte[6] { 255, 255, 255, 255, 255, 255 }; // 데드락 ( 255가 아닌 값...)


    public int order = 0;

    /*
        0 = C;
        1 = A;
        2 = B;
    */

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        RecvThread_Start = new Thread(RecvFunc);
        RecvThread_Start.Start();
    }

    void OnEnable()
    {
        Texture2D txt2d = Resources.Load("CardImage/" + byteToBinary(DumpCard)) as Texture2D;
        dump.GetComponent<Image>().sprite = Sprite.Create(Resources.Load("CardImage/" + byteToBinary(DumpCard)) as Texture2D, new Rect(0, 0, txt2d.width, txt2d.height), new Vector2(0.5f, 0.5f));

        /*
            문제
        */

        bool[] check = new bool[14];

        for(int i = 0; i < check.Length; i++)
        {
            check[i] = false;
        }




        Queue<byte>[] normal = new Queue<byte>[7];

        for (int i = 0; i < normal.Length; i++)
        {
            normal[i] = new Queue<byte>();
        }


        byte[] king = new byte[7];


        for (int i = 0; i < MyCardList_Length; i++)
        {
            int a = MyCardList[i]/8;

            if(a % 2 == 0)
            {
                check[a / 2] = true;
                normal[a / 2].Enqueue(MyCardList[i]);
            }
            else
            {
                check[8 + a / 2] = true;
                king[a / 2] = MyCardList[i];
            }
        }

        //패의 카드 이니시에이팅
        for(int i = 0; i < 14; i++)
        {
            Transform trsfm = hand.Find("Card (" + i.ToString() + ")");
            if (check[i])
            {
                if(i < 7)
                {
                    trsfm.GetComponent<Card>().normal = normal[i];

                    txt2d = Resources.Load("CardImage/" + byteToBinary(normal[i].Peek())) as Texture2D;
                    trsfm.GetComponent<Image>().sprite = Sprite.Create(Resources.Load("CardImage/" + byteToBinary(normal[i].Peek())) as Texture2D, new Rect(0, 0, txt2d.width, txt2d.height), new Vector2(0.5f, 0.5f));
                    //카드에 큐를 줌
                }
                else
                {
                    trsfm.GetComponent<Card>().isKing = true;
                    trsfm.GetComponent<Card>().king = king[i];

                    txt2d = Resources.Load("CardImage/" + byteToBinary(DumpCard)) as Texture2D;
                    trsfm.GetComponent<Image>().sprite = Sprite.Create(Resources.Load("CardImage/" + byteToBinary(DumpCard)) as Texture2D, new Rect(0, 0, txt2d.width, txt2d.height), new Vector2(0.5f, 0.5f));
                    //카드에 바이트를 줌
                }

                trsfm.gameObject.SetActive(true);
            }
            else
            {
                Destroy(trsfm.gameObject);
            }
        }
    }

    static string byteToBinary(byte code)
    {
        return System.Convert.ToString(code, 2).PadLeft(8, '0');
    }

    public Socket socket;
    Thread RecvThread_Start;

    // Use this for initialization
   

    void RecvFunc()
    {
        Debug.Log("Test");
        socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777));
        while (RecvData[0] != 7)
        {
            RecvData_Length = socket.Receive(RecvData, 0, RecvData.Length, SocketFlags.None);

            if (RecvData[0] == 0) // 0 번호 인원
            {
                MyNumber = RecvData[1];
                MaxPlayer = RecvData[2];
                if (MyNumber == 0)
                {
                    first = true;
                }
            }
            else if (RecvData[0] == 1) // 1 Target CardName Q
            {
                QuestionTarget_R = RecvData[1];
                QuestionCardName_R = RecvData[2];
                QuestionQ_R = RecvData[3];
            }
            else if (RecvData[0] == 2) // 2 Target A
            {
                AnswerTarget_R = RecvData[1];
                AnswerA_R = RecvData[2];
            }
            else if (RecvData[0] == 3) // 3 who CardCount [ ] who CardCount [ ] ...
            {
                GraveCard[RecvData[1]] = new byte[RecvData_Length - 2];
                for (int i = 2, k = 0; i < RecvData_Length; i++, k++)
                    GraveCard[RecvData[1]][k] = RecvData[i];

            }
            else if (RecvData[0] == 4) // 4 who CardCount who CardCount ...
            {
                for (int i = 2, k = 0; k < MaxPlayer; i += 2)
                {
                    PlayerCards[k] = RecvData[i];
                }
            }
            else if (RecvData[0] == 5) // 5 카드 갱신
            {
                MyCardList_RecvData = RecvData;
                MyCardList_Length = RecvData_Length;
            }
            else if (RecvData[0] == 6) // Winnder
            {

            }
            else if (RecvData[0] == 7) // Loser - 게임 종료
            {

            }
            else if (RecvData[0] == 8) // 8 PassLock
            {
                for (int i = 1, k = 0; i < RecvData_Length; i++, k++)
                    PassLock[k] = RecvData[i];
            }
            else if (RecvData[0] == 9) // Dump Card
            {
                DumpCard = RecvData[1];
            }
        }
    }

    public bool first = false;
    public bool answerCheck = false;

    public GameObject[] objects;

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if(order == 0)
        {

        }
        else if(order == 1)
        {
            List<Selectable> selectable = new List<Selectable>();

            for(int i = 0; i < hand.transform.childCount; i++)
            {
                selectable.Add(hand.GetChild(i).GetComponent<Selectable>());
                Debug.Log(hand.GetChild(i).GetComponent<Selectable>());
            }

            foreach(Selectable i in selectable)
            {
                i.enabled = false;
            }

            if (first && OK_Question)
            {
                socket.Send(QuestionData, 0, QuestionData.Length, SocketFlags.None);
                OK_Question = false;
            }
        }
        else if(order == 2)
        {
            if (QuestionTarget_R == MyNumber && answerCheck)
            {
                socket.Send(AnswerData, 0, AnswerData.Length, SocketFlags.None);
                answerCheck = false;
            }
        }
        else
        {
            Application.Quit();
        }
    }
}
