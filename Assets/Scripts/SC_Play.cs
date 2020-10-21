//using System;
using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SC_Play : MonoBehaviour
{
    // tokenObjectBlack has image to CircleBloack
    // tokenObjectWhite has image to CiecleWhite
    public Sprite tokenObjectBlack, tokenObjectWhite;
    // buttonObjects has all GameObjects(buttons) with tag "ButtonObject"
    public Dictionary<string, GameObject> buttonObjects;
    // textCounterObjects has all (just three, here) GameObjects(texts) with tag "TextCounter"
    public Dictionary<string, GameObject> textCounterObjects;
    // gameover has all GameObjects (in gameover screen/panel) with tag "GameOver"
    private Dictionary<string, GameObject> gameover;
    // clickedObject has all full cells (not empty)
    private Dictionary<int, GameObject> clickedObjects;
    // curBoard id list, each cell in it represents a slot in the board representing whether it is empty or not...
    private List<GlobalEnums.SloState> curBoard;
    // curState represents the color (Circle), will now play with
    private GlobalEnums.SloState curState;

    private GlobalEnums.SloState playerState;
    // blackCounter and WhiteCounter represent the amount of white or black Circles on the board
    private int blackCounter, whiteCounter;

    private float startTimer = 0;
    private bool gameStarted = false;
    private string curTurn = string.Empty;
    private bool isMyTurn = false;
    #region Singleton
    static SC_Play instance;
    public static SC_Play Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.Find("SC_Play").GetComponent<SC_Play>();
            }
            return instance;
        }
    }
    #endregion
    #region MonoBehaviour
    private void OnEnable()
    {
        Listener.OnGameStarted += OnGameStarted;
        Listener.OnGameStopped += OnGameStopped;
        Listener.OnMoveCompleted += OnMoveCompleted;
    }
    private void OnDisable()
    {
        Listener.OnGameStarted -= OnGameStarted;
        Listener.OnMoveCompleted -= OnMoveCompleted;
        Listener.OnGameStopped += OnGameStopped;
    }
    public void Awake()
    {
        Init();
    }
    private void Update()
    {
        Timer();
    }
    #endregion
    #region Logic
    private void Init() {
        buttonObjects = new Dictionary<string, GameObject>();
        textCounterObjects = new Dictionary<string, GameObject>();
        curBoard = new List<GlobalEnums.SloState>();
        clickedObjects = new Dictionary<int, GameObject>();
        gameover = new Dictionary<string, GameObject>();

        GameObject[] _objects = GameObject.FindGameObjectsWithTag("ButtonObject");
        foreach (GameObject g in _objects)
        {
            buttonObjects.Add(g.name, g);
        }
        _objects = GameObject.FindGameObjectsWithTag("GameOver");
        foreach (GameObject g in _objects)
        {
            gameover.Add(g.name, g);
        }
        _objects = GameObject.FindGameObjectsWithTag("TextCounter");
        foreach (GameObject g in _objects)
        {
            textCounterObjects.Add(g.name, g);
        }
        Reset();
    }

    public void Reset()
    {
        gameover["Screen_GameOver"].SetActive(false);
        gameover["Screen_RestartQ"].SetActive(false);
        blackCounter = 0; whiteCounter = 0;
        Debug.Log(clickedObjects.Count);
        Debug.Log(curBoard.Count);
        clickedObjects.Clear();
        curBoard.Clear();
        Debug.Log(clickedObjects.Count);
        Debug.Log(curBoard.Count);
        clickedObjects.Add(28, buttonObjects["Btn_28"]);
        clickedObjects.Add(29, buttonObjects["Btn_29"]);
        clickedObjects.Add(36, buttonObjects["Btn_36"]);
        clickedObjects.Add(37, buttonObjects["Btn_37"]);
        curBoard.Clear();
        for (int i = 1; i <= GlobalVariables.slotAmount; i++)
        {
            curBoard.Add(GlobalEnums.SloState.Empty);
            buttonObjects["Btn_" + i].GetComponent<SC_Slot>().ChangeSlotState(GlobalEnums.SloState.Empty);
            buttonObjects["Btn_" + i].GetComponent<Button>().interactable = false;
        }
        int[] _basicIndex1 = { 28, 37 };
        foreach (int i in _basicIndex1)
        {
            buttonObjects["Btn_" + i].GetComponent<SC_Slot>().ChangeSlotState(GlobalEnums.SloState.Black);
            curBoard[i - 1] = GlobalEnums.SloState.Black;
        }
        int[] _basicIndex2 = { 29, 36 };
        foreach (int i in _basicIndex2)
        {
            buttonObjects["Btn_" + i].GetComponent<SC_Slot>().ChangeSlotState(GlobalEnums.SloState.White);
            curBoard[i - 1] = GlobalEnums.SloState.White;
        }
        OpenArea();
        if (GlobalVariables.curOpponent == GlobalEnums.Opponent.AI) {
            textCounterObjects["Text_TurnTimer"].GetComponent<Text>().text = string.Empty;
            if (Random.Range(0, 2) == 0)
            {   // AI
                curState = GlobalEnums.SloState.White;
                buttonObjects["Img_CurState"].GetComponent<Image>().sprite = tokenObjectWhite;

                int _idx = GetRandomSlot();
                PlacementLogic(_idx);
            }
            else
            {
                // Player
                curState = GlobalEnums.SloState.Black;
                buttonObjects["Img_CurState"].GetComponent<Image>().sprite = tokenObjectBlack;
            }
        }
    }

    private void PassTurn()
    {
        if (curState == GlobalEnums.SloState.Black)
        {
            curState = GlobalEnums.SloState.White;
            buttonObjects["Img_CurState"].GetComponent<Image>().sprite = tokenObjectWhite;
        }
        else {
            curState = GlobalEnums.SloState.Black;
            buttonObjects["Img_CurState"].GetComponent<Image>().sprite = tokenObjectBlack;
        }
    }
    private GlobalEnums.MatchState IsMatchOver()
    {
        int _emptyCounter = 0;
        blackCounter = 0; whiteCounter = 0;
        foreach (GlobalEnums.SloState val in curBoard)
        {
            if (val == GlobalEnums.SloState.Black)
            {
                blackCounter++;
            }
            else if (val == GlobalEnums.SloState.White)
            {
                whiteCounter++;
            }
            else _emptyCounter++;
        }
        Debug.Log("Empty:" + _emptyCounter);
        if (_emptyCounter != 0 && (blackCounter != 0 && whiteCounter != 0))
            return GlobalEnums.MatchState.NoWinner;
        if (blackCounter == 32 && whiteCounter == 32)
            return GlobalEnums.MatchState.Tie;
        return GlobalEnums.MatchState.Winner;
    }
    private void OpenArea()
    {
        int[] _areaKey = { -9, -8, -7, -1, +1, +7, +8, +9 };
        foreach (int key in clickedObjects.Keys)
        {
            foreach (int i in _areaKey)
            {
                if ((key % 8 == 0 && (i == 1 || i == 9 || i == -7))
                    || (key % 8 == 1 && (i == -1 || i == -9 || i == 7)))
                    continue;
                int _index = key + i;
                if (clickedObjects.ContainsKey(_index))
                    continue;
                else if (0 < _index && _index <= GlobalVariables.slotAmount)
                {
                    Debug.Log(_index);
                    if (buttonObjects["Btn_" + _index].GetComponent<Button>().interactable == false)
                        buttonObjects["Btn_" + _index].GetComponent<Button>().interactable = true;
                }
            }
        }
    }
    private void ChangeCircles(int _Index)
    {
        //
        Change(_Index, -1, 1, _Index % 8 == 0); // Right
        Change(_Index, +1, 2, _Index % 8 == 1); // Left
        Change(_Index, -8, 3);  //Up 
        Change(_Index, +8, 4);  //Down
        Change(_Index, -7, 3, false, 1); // Left-Up 
        Change(_Index, +7, 4, false, 2); // Right-Down
        Change(_Index, -9, 3, false, 3); // Right-Up
        Change(_Index, +9, 4, false, 4); // Left-Down
    }

    public void StartGame()
    {
        if(GlobalVariables.curOpponent == GlobalEnums.Opponent.Player)
        {
            WarpClient.GetInstance().startGame();
        }
        Reset();
    }
   
    private GlobalEnums.MatchState PlacementLogic(int _Number)
    {
        GlobalEnums.MatchState _isOver = GlobalEnums.MatchState.NoWinner;
        Debug.Log("Btn_" + _Number);
        Debug.Log(curBoard[_Number - 1]);
        if (curBoard[_Number - 1] == GlobalEnums.SloState.Empty)
        {
            curBoard[_Number - 1] = curState;
            buttonObjects["Btn_" + _Number].GetComponent<SC_Slot>().ChangeSlotState(curState);
            clickedObjects.Add(_Number, buttonObjects["Btn_" + _Number]);
            buttonObjects["Btn_" + _Number].GetComponent<Button>().interactable = false;
            ChangeCircles(_Number);
            _isOver = IsMatchOver();
            textCounterObjects["Text_CounterBlack"].GetComponent<Text>().text = (blackCounter < 10) ? "0" + blackCounter : blackCounter.ToString();
            textCounterObjects["Text_CounterWhite"].GetComponent<Text>().text = (whiteCounter < 10) ? "0" + whiteCounter : whiteCounter.ToString();
            if (_isOver != GlobalEnums.MatchState.NoWinner)
            {
                Debug.Log("_isOver = " + _isOver);
                ChangeInteration();
                gameover["Screen_GameOver"].SetActive(true);
                if (_isOver == GlobalEnums.MatchState.Winner)
                {
                    if (blackCounter > whiteCounter)
                    {
                        gameover["Img_Winner"].GetComponent<Image>().sprite = tokenObjectBlack;
                    }
                    else gameover["Img_Winner"].GetComponent<Image>().sprite = tokenObjectWhite;
                }
                else
                {
                    gameover["Img_Winner"].SetActive(false);
                    gameover["Text_Winner"].GetComponent<Text>().text = "No Winner / Tie";

                }
                gameStarted = false;
                textCounterObjects["Text_TurnTimer"].GetComponent<Text>().text = string.Empty;
                
            }
            OpenArea();
            PassTurn();

            if (GlobalVariables.curOpponent == GlobalEnums.Opponent.AI &&
                curState == GlobalEnums.SloState.White && _isOver == GlobalEnums.MatchState.NoWinner)
                StartCoroutine(PlayAI());

        }
        return _isOver;
    }
    private int GetRandomSlot()
    {

        Debug.Log("GetRandomSlot");
        List<int> _list = new List<int>();
        for (int i = 1; i <= GlobalVariables.slotAmount; i++)
        {
            if (buttonObjects["Btn_" + i].GetComponent<Button>().interactable == true)
                _list.Add(i);
        }
        int _rand = Random.Range(0, _list.Count);
        Debug.Log("EndGetRandomSlot");
        Debug.Log("_list.Count " + _list.Count);
        Debug.Log("_rand " + _rand);
        return _list[_rand];
    }
    private IEnumerator PlayAI()
    {
        yield return new WaitForSeconds(0.5f);
        int _idx = GetRandomSlot();
        PlacementLogic(_idx);
    }

    private void Timer()
    {
        if (gameStarted)
        {
            float _curTime = Time.time - startTimer;
            int _leftTime = (int)(GlobalVariables.turnTimer - _curTime);
            if (_leftTime > 0)
                textCounterObjects["Text_TurnTimer"].GetComponent<Text>().text = (_leftTime > 9) ? _leftTime.ToString() : "0" + _leftTime.ToString();
            else
            {
                textCounterObjects["Text_TurnTimer"].GetComponent<Text>().text = "00";
                PassTurn();
                startTimer = Time.time;
                if (isMyTurn)
                    isMyTurn = false;
                else isMyTurn = true;
            }
        }
        
    }
    #endregion
    #region Events
    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        Reset();
        startTimer = Time.time;
        gameStarted = true;
        curTurn = _NextTurn;
        curState = GlobalEnums.SloState.Black;
        buttonObjects["Img_CurState"].GetComponent<Image>().sprite = tokenObjectBlack;
        if (curTurn == GlobalVariables.userID)
        {
            isMyTurn = true;
            playerState = GlobalEnums.SloState.Black;
            buttonObjects["Img_MyCircle"].GetComponent<Image>().sprite = tokenObjectBlack;
        }
        else
        {
            isMyTurn = false;
            playerState = GlobalEnums.SloState.White;
            buttonObjects["Img_MyCircle"].GetComponent<Image>().sprite = tokenObjectWhite;
        }
    }
    private void OnGameStopped(string _Sender, string _RoomId)
    {
        Debug.Log("OnGameStopped " + _Sender + " " + _RoomId);
    }
    private void OnMoveCompleted(MoveEvent _Move)
    {
        startTimer = Time.time;
        if (_Move.getSender() != GlobalVariables.userID)
        {
            Dictionary<string, object> _data = (Dictionary<string, object>) MiniJSON.Json.Deserialize(_Move.getMoveData());
            
            if (_data != null && _data.ContainsKey("Index"))
            {
                if (_data.ContainsKey("Index"))
                {
                    int _index = int.Parse(_data["Index"].ToString());
                    if (_index > 0 && gameStarted)
                    {
                        GlobalEnums.MatchState _currState = PlacementLogic(_index);
                        if (_currState != GlobalEnums.MatchState.NoWinner)
                            gameStarted = false;
                    }
                    else if (_index < 0)
                    {
                        gameover["Screen_RestartQ"].SetActive(true);
                        WarpClient.GetInstance().stopGame();
                    }
                }
            }
            if (_Move.getNextTurn() == GlobalVariables.userID)
                isMyTurn = true;
            else isMyTurn = false;
        }
    }
    #endregion
    #region Controller
    public void Btn_SlotPlay(int _Number)
    {
        Debug.Log("AI or Player: " + GlobalVariables.curOpponent);
        if ((curState == GlobalEnums.SloState.Black && GlobalVariables.curOpponent == GlobalEnums.Opponent.AI)
            || (isMyTurn && GlobalVariables.curOpponent == GlobalEnums.Opponent.Player)
            || _Number == -1 && GlobalVariables.curOpponent == GlobalEnums.Opponent.Player)
            //last line mean, clicked about restart button
        {
            isMyTurn = false;
            if(_Number > 0)
                PlacementLogic(_Number);
            Dictionary<string, object> _toSend = new Dictionary<string, object>();
            _toSend.Add("Index", _Number);
            string _sendData = MiniJSON.Json.Serialize(_toSend);
            WarpClient.GetInstance().sendMove(_sendData);
            Debug.Log("Sent Data: " + _sendData);
        }
    }
    public void Restart()
    {
        if (GlobalVariables.curOpponent == GlobalEnums.Opponent.Player)
        {
            Btn_SlotPlay(-1);
        }
        else StartGame();
    }
    #endregion
    #region FunctionsHelp
    private void Change(int _Index, int move ,int number, bool b = false, int num = 0)
    {
        List<int> _indexs = new List<int>();
        bool canChange = false, mainBool = false, q = false;
        int _index = _Index, counter = 0;

        switch (number)
        {
            case 1: mainBool = _index % 8 != 0; break;
            case 2: mainBool = _index % 8 != 1; break;
            case 3: mainBool = _index > 0; break;
            case 4: mainBool = _index <= 64; break;
        }
        switch (num)
        {
            case 1: q = (_index + 7) % 8 == 1; break;
            case 2: q = (_index - 7) % 8 == 0; break;
            case 3: q = (_index + 9) % 8 == 1; break;
            case 4: q = (_index - 9) % 8 == 0; break;
        }
        while (mainBool || (b && _index > 0 && _index <=64))
        {
            Debug.Log(_index);

            if (_index != _Index)
            {
                if (curBoard[_index - 1] == GlobalEnums.SloState.Empty || q)
                    break;
                else if (curBoard[_index - 1] == curBoard[_Index - 1])
                {
                    canChange = true;
                    break;
                }
                _indexs.Add(_index);
                counter++; 
            }
            _index += move;
            switch (number)
            {
                case 1: mainBool = _index % 8 != 0; break;
                case 2: mainBool = _index % 8 != 1; break;
                case 3: mainBool = _index > 0; break;
                case 4: mainBool = _index <= 64; break;
            }
            switch (num)
            {
                case 1: q = (_index + 7) % 8 == 1; break;
                case 2: q = (_index - 7) % 8 == 0; break;
                case 3: q = (_index + 9) % 8 == 1; break;
                case 4: q = (_index - 9) % 8 == 0; break;
            }
            Debug.Log(mainBool);
            Debug.Log(b);
        }
        Debug.Log("Counter : " + counter);
        if (canChange == true)
        {
            foreach (int val in _indexs)
            {
                Debug.Log(val);
                curBoard[val - 1] = curBoard[_Index - 1];
                buttonObjects["Btn_" + val].GetComponent<SC_Slot>().ChangeSlotState(curState);
            }
        }
    }
    private void ChangeInteration(bool _IsActive = false)
    {
        for(int i = 1; i <= GlobalVariables.slotAmount; i++)
            buttonObjects["Btn_" + i].GetComponent<Button>().interactable = _IsActive;
    }
    #endregion
}