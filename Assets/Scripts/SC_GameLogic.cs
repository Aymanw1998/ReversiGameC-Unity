using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class SC_GameLogic : MonoBehaviour
{
    // screenObjects -> has Game Objects with tag "ScreenObject"
    private Dictionary<string, GameObject> screenObjects;
    // anotherUnityObjects -> has all Game Object with tag "UnityObject" (for the menu)
    private Dictionary<string, GameObject> anotherUnityObjects;
    // pathScreens -> save the path cause when we click back so we can back to correct srceen
    private Stack<GameObject> pathScreens;
    
    #region Singleton
    static SC_GameLogic instance;
    public static SC_GameLogic Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.Find("SC_GameLogic").GetComponent<SC_GameLogic>();
            }
            return instance;
        }
    }
    #endregion
    #region MonoBehaviour
    
    
    public void Awake()
    {
        Init();
    }
    #endregion
    #region Logic
    private void Init()
    {

        anotherUnityObjects = new Dictionary<string, GameObject>();
        screenObjects = new Dictionary<string, GameObject>();
        pathScreens = new Stack<GameObject>();
       
        //push or add all Game Object with tag "UnityObject" to anotherUnityObjects
        GameObject[] _anotherUnityObjects = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _anotherUnityObjects)
        {
            anotherUnityObjects.Add(g.name, g);
            Debug.Log(g.name);
        } 

        //push or add all Game Object with tag "screenObjects" to anotherUnityObjects just not "Screen_menu"
        //push Screen_Menu to the Stack "pathScreens" (first element)
        GameObject[] _screenObjects = GameObject.FindGameObjectsWithTag("ScreenObject");
        foreach (GameObject g in _screenObjects)
        {
            screenObjects.Add(g.name, g);
            if (g.name != "Screen_Menu")
                screenObjects[g.name].SetActive(false);
            else pathScreens.Push(g);
        }

       
    }

    #endregion
    #region Controller
    public void Btn_Screen(string _Screen)
    {
        //Btn_RestartLogic();
        ChangeScreen(_Screen);
    }
    private void ChangeScreen(string _NewScreen)
    {
        //pop the top element from the stack
        GameObject _tmp = pathScreens.Pop();
        //Push it again to the Stack
        pathScreens.Push(_tmp);
        /* we do this becuase we want save the top element in _tmp*/

        //Push the new Screen to the Stack
        pathScreens.Push(screenObjects[_NewScreen]);

        //off the past (_tmp) Screen
        _tmp.SetActive(false);
        //on the new Screen
        screenObjects[_NewScreen].SetActive(true);
    }
    public void Btn_BackLogic()
    {
        string _nowScreen = string.Empty;
        // pop the Screen (end in path) and off it
        screenObjects[_nowScreen = pathScreens.Pop().name].SetActive(false);

        if (_nowScreen == "Screen_Multiplayer")
        {
            SC_Play.Instance.Reset();
            SC_Multiplayer.Instance.Btn_Go_Out();
        }
        //pop the top element from the stack
        GameObject _tmp = pathScreens.Pop();

        //Push it again to the Stack
        pathScreens.Push(_tmp);
        /* we do this becuase we want save the top element in _tmp*/

        //on the back Screen
        screenObjects[_tmp.name].SetActive(true);
    }
    public void Slider_RandomNumberLogic()
    {
        int _value = (int)SC_GameLogic.Instance.anotherUnityObjects["Slider_RandomNumber"].GetComponent<Slider>().value;
        anotherUnityObjects["Text_RandomNumber"].GetComponent<Text>().text = _value.ToString();
    }
    public void Btn_ConnictionLogic()
    {
        if (SC_Multiplayer.Instance.Btn_ConnictionMulti())
            ChangeScreen("Screen_Multiplayer");
    }

    public void Btn_LogoutLogic()
    {
        SC_Multiplayer.Instance.Btn_LogoutMulti();

    }

    public void Btn_SlotLogic(int _Number)
    {
        SC_Play.Instance.Btn_SlotPlay(_Number);
    }
    public void Btn_RestartLogic()
    {
        SC_Play.Instance.Restart();
    }
    public void Btn_YesLogic()
    {
        SC_Play.Instance.StartGame();
    }
    public void Btn_NoLogic()
    {
        SC_Play.Instance.Reset();
        if (GlobalVariables.curOpponent == GlobalEnums.Opponent.AI)
            Btn_Screen("Screen_Singleplayer");
        else
        {
            Btn_Screen("Screen_Multiplayer");
            SC_Multiplayer.Instance.Btn_Go_Out();
        }
    }
    public void Btn_Play_MultiplayerLogic()
    {
        SC_Multiplayer.Instance.Btn_Play();
    }
    public void Btn_Go_Out_MultiplayerLogic()
    {
        SC_Multiplayer.Instance.Btn_Go_Out();
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
