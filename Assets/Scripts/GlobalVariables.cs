using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public static int slotAmount = 64;
    public static int turnTimer = 30;
    public static string userID = string.Empty;
    public static GlobalEnums.Opponent curOpponent = GlobalEnums.Opponent.AI;
}
