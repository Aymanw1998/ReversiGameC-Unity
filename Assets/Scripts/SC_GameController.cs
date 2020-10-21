using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;

public class SC_GameController : MonoBehaviour
{
    
    public void Btn_Singleplayer()
    {
        SC_GameLogic.Instance.Btn_Screen("Screen_Game");
    }
  
    public void Btn_Multiplayer()
    {

        SC_GameLogic.Instance.Btn_Screen("Screen_Conniction");
    }
    
    public void Btn_Logout()
    {
        SC_GameLogic.Instance.Btn_LogoutLogic();
    }
   
    public void Btn_Back()
    {
        SC_GameLogic.Instance.Btn_BackLogic();
    }
    
    public void Btn_GoConniction()
    {
        SC_GameLogic.Instance.Btn_ConnictionLogic();
    }
    
    public void Slider_RandomNumber()
    {
        SC_GameLogic.Instance.Slider_RandomNumberLogic();
    }
    public void Btn_Slot(int _Number)
    {
        SC_GameLogic.Instance.Btn_SlotLogic(_Number);
    }
    public void Btn_Play_Multiplayer()
    {
        SC_GameLogic.Instance.Btn_Play_MultiplayerLogic();
    }
    public void Btn_Go_Out_Multiplayer()
    {
        SC_GameLogic.Instance.Btn_Go_Out_MultiplayerLogic();
    }
    public void Btn_Restart()
    {
        SC_GameLogic.Instance.Btn_RestartLogic();
    }
    public void Btn_Yes()
    {
        SC_GameLogic.Instance.Btn_YesLogic();
    }
    public void Btn_No()
    {
        SC_GameLogic.Instance.Btn_NoLogic();
    }
}
