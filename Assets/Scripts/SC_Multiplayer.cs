using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SC_Multiplayer : MonoBehaviour
{
    //apikey and sceretkey becuase we want share with Waprap
    private string apiKey = "0cbeedca6c116e367be5c586d00dd4d37f5ae2ce1a0d3d4d21edc336383da1ff";
    private string sceretKey = "bbdb2eff9322520890a7aee95082f26c58dc24c41be3991173a457572a8a5740";
    private Listener listener;
    //roomIds has all rooms 
    private List<string> roomIds;
    private int roomIdx = 0;
    private string roomId = "";
    private int indexRoom = 0;
    Dictionary<string, GameObject> unityObjects;
    private Dictionary<string, object> matchRoomData;
    
    #region Singelton
    static SC_Multiplayer instance;
    public static SC_Multiplayer Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_Multiplayer").GetComponent<SC_Multiplayer>();
            return instance;
        }
    }
    #endregion

    #region MonoBehaviour
    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnGameStarted += OnGameStarted;
    }
    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnGameStarted -= OnGameStarted;
    }
    void Awake()
    {
        Init();
    }
    #endregion
    #region Logic
    private void Init()
    {

        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _unityObject = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _unityObject)
        {
            unityObjects.Add(g.name, g);
        }
        matchRoomData = new Dictionary<string, object>();
        unityObjects["Text_UserID"].SetActive(false);
        unityObjects["Btn_Logout"].SetActive(false);
        unityObjects["Text_StatusUser"].SetActive(false);
        if (listener == null)
            listener = new Listener();

        WarpClient.initialize(apiKey, sceretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listener);
        WarpClient.GetInstance().AddChatRequestListener(listener);
        WarpClient.GetInstance().AddRoomRequestListener(listener);
        WarpClient.GetInstance().AddUpdateRequestListener(listener);
        WarpClient.GetInstance().AddLobbyRequestListener(listener);
        WarpClient.GetInstance().AddNotificationListener(listener);
        WarpClient.GetInstance().AddRoomRequestListener(listener);
        WarpClient.GetInstance().AddZoneRequestListener(listener);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listener);

        UpdateStatus("Connecting...");
    }
    public void RestLogic()
    {
        unityObjects["Text_RandomNumber"].GetComponent<Text>().text = string.Empty;
        UpdateStatus("Connecting...");
        if (roomIds != null && roomIds.Count > 0)
            roomIds.Clear();
        matchRoomData.Clear();
        roomIdx = 0;
        roomId = "";
        indexRoom = 0;
        unityObjects["Btn_Play"].GetComponent<Button>().interactable = true;
        unityObjects["Text_UserID"].SetActive(false);
        unityObjects["Text_StatusUser"].SetActive(false);
        unityObjects["Btn_Logout"].SetActive(false);
    }
    private void UpdateStatus(string _NewStatus)
    {
        unityObjects["Text_StatusUser"].GetComponent<Text>().text = _NewStatus;
    }
    private void DoRoomSearchLogic()
    {
        if (roomIds.Count > 0 && roomIds.Count > roomIdx)
        {
            //Get Room information
            UpdateStatus("Getting room Details: " + roomIds[roomIdx]);
            WarpClient.GetInstance().GetLiveRoomInfo(roomIds[roomIdx]);
        }
        else
        {
            //Create Room
            Debug.Log("matchRoomData: " + matchRoomData.Count);
            if (matchRoomData.Count > 0 && !matchRoomData.ContainsKey(""))
            {
                indexRoom++;
                UpdateStatus("Creating as room...");
                WarpClient.GetInstance().CreateTurnRoom(GlobalVariables.userID + indexRoom, GlobalVariables.userID, 2, matchRoomData, GlobalVariables.turnTimer);
            }
            else
            {
                UpdateStatus("Choose Number ...");
                Btn_Go_Out();
            }
        }
    }
    #endregion
    #region Events

    private void OnConnect(bool _IsSuccess)
    {
        Debug.Log(_IsSuccess);
        if (_IsSuccess)
        {
            UpdateStatus("Connected.");

            unityObjects["Text_UserID"].SetActive(true);
            unityObjects["Btn_Play"].GetComponent<Button>().interactable = true;
            unityObjects["Btn_Logout"].SetActive(true);
            unityObjects["Btn_Logout"].SetActive(true);
            unityObjects["Text_StatusUser"].SetActive(true);
        }
        else
        {
            UpdateStatus("Failed to connect.");
            unityObjects["Btn_Play"].GetComponent<Button>().interactable = true;
            RestLogic();
            SC_GameLogic.Instance.Btn_Screen("Screen_Conniction");
        }
    }
    private void OnRoomsInRange(bool _IsSuccess, MatchedRoomsEvent evetObj)
    {
       // Debug.Log(_IsSuccess + " " + evetObj.getRoomsData().Length);
        if (_IsSuccess)
        {
            UpdateStatus("Parsing Rooms.");
            roomIds = new List<string>();
            foreach (var roomData in evetObj.getRoomsData())
            {
                Debug.Log("Room Id " + roomData.getId());
                Debug.Log("Room Owner " + roomData.getRoomOwner());
                roomIds.Add(roomData.getId());
            }

            roomIdx = 0;
            DoRoomSearchLogic();


        }
        else
        {
            UpdateStatus("Error fetching room data");
            unityObjects["Btn_Play"].GetComponent<Button>().interactable = true;
        }
    }
    private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        Debug.Log(_IsSuccess + " " + _RoomId);
        if (_IsSuccess)
        {
            unityObjects["Text_RoomID"].GetComponent<Text>().text = "RoomId: " + _RoomId;
            UpdateStatus("Room Created, Waiting for an opponnent");
            roomId = _RoomId;
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
    }
    private void OnGetLiveRoomInfo(LiveRoomInfoEvent _EventObj)
    {
        Dictionary<string, object> _params = _EventObj.getProperties();
        string _number = unityObjects["Text_RandomNumber"].GetComponent<Text>().text;
        Debug.Log("Text_RandomNumber: "+_params.ContainsKey(_number));
        Debug.Log("RandomNumber: "+ _number);
        if (_params.ContainsKey(_number) && matchRoomData.ContainsKey(_number) &&
            _params[_number].ToString() == matchRoomData[_number].ToString()) 
        {   
            roomId = _EventObj.getData().getId();
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
        else
        {
            roomIdx++; DoRoomSearchLogic();
        }
    }
    private void OnUserJoinRoom(RoomData _EventObj, string _UserId)
    {
        UpdateStatus("User: " + _UserId + " has joined the room");
        if (_UserId != GlobalVariables.userID)
        {
            //Start Game
            Debug.Log("Start Game");
            UpdateStatus("Starting Game ....");
            WarpClient.GetInstance().startGame();

        }
        unityObjects["Btn_Play"].GetComponent<Button>().interactable = true;
    }
    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        if (_IsSuccess)
        {
            UpdateStatus("Joiend Room: " + _RoomId);
            unityObjects["Text_RoomID"].GetComponent<Text>().text = "RoomId: " + _RoomId;
            
        }
    }
    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        UpdateStatus("Game Started, Turn: " + _NextTurn);
        SC_GameLogic.Instance.Btn_Screen("Screen_Game");
        unityObjects["Btn_Logout"].GetComponent<Button>().interactable = false;

    }

    #endregion
    #region Controller

    public void Btn_Play()
    {
        GlobalVariables.curOpponent = GlobalEnums.Opponent.Player;
        unityObjects["Btn_Logout"].GetComponent<Button>().interactable = false;
        unityObjects["Btn_Play"].GetComponent<Button>().interactable = false;
        unityObjects["Btn_Go_Out"].GetComponent<Button>().interactable = true;
        if (!matchRoomData.ContainsKey(unityObjects["Text_RandomNumber"].GetComponent<Text>().text) || unityObjects["Text_RandomNumber"].GetComponent<Text>().text != string.Empty)
            matchRoomData.Add(unityObjects["Text_RandomNumber"].GetComponent<Text>().text, unityObjects["Text_RandomNumber"].GetComponent<Text>().text);
        UpdateStatus("Searching for room...");
        WarpClient.GetInstance().GetRoomsInRange(1, 2);
    }
    public void Btn_Go_Out()
    {
        WarpClient.GetInstance().DeleteRoom(roomId);
        UpdateStatus("connected.");
        unityObjects["Btn_Logout"].GetComponent<Button>().interactable = true;
        unityObjects["Btn_Play"].GetComponent<Button>().interactable = true;
        unityObjects["Btn_Go_Out"].GetComponent<Button>().interactable = false;
        unityObjects["Text_RandomNumber"].GetComponent<Text>().text = string.Empty;
        Debug.Log("matchRoomData: " + matchRoomData.Count);
        Debug.Log("roomIds: " + roomIds.Count);
        if (!matchRoomData.ContainsKey(unityObjects["Text_RandomNumber"].GetComponent<Text>().text) || unityObjects["Text_RandomNumber"].GetComponent<Text>().text == string.Empty)
        {
            matchRoomData.Clear();
            if (roomIds != null && roomIds.Count > 0)
                roomIds.Clear();
            roomIdx = 0;

            Debug.Log("matchRoomDataGoOut: " + matchRoomData.Count);
            Debug.Log("roomIdsGoOut: " + roomIds.Count);
        }
    }
    public bool Btn_ConnictionMulti()
    {
        GlobalVariables.userID = unityObjects["InputField_ID_User"].GetComponent<InputField>().text;
        if (GlobalVariables.userID != "")
        {
            unityObjects["Text_UserID"].GetComponent<Text>().text = "ID: " + GlobalVariables.userID;
            unityObjects["Text_StatusUser"].SetActive(true);
            WarpClient.GetInstance().Connect(GlobalVariables.userID);
            return true;
        }
        return false;
    }
    public void Btn_LogoutMulti()
    {
        RestLogic();
        SC_GameLogic.Instance.Btn_Screen("Screen_Menu");
        WarpClient.GetInstance().Disconnect();
    }
    #endregion
    #region Example
    private void JSONExample()
    {

        Dictionary<string, object> _tmpDictionary = new Dictionary<string, object>();
        _tmpDictionary.Add("Password", "aw199816");
        _tmpDictionary.Add("Player", 1);
        _tmpDictionary.Add("isActive", true);

        string _toSend = MiniJSON.Json.Serialize(_tmpDictionary);

        Debug.Log(_toSend);

        Dictionary<string, object> _parsedDictionary = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_toSend);

        Debug.Log(_parsedDictionary);
    }
    #endregion
}
