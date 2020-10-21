using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEnums : MonoBehaviour
{
    public enum SloState
    {
        Empty,Black,White
    };

    public enum MatchState
    {
        NoWinner,Winner,Tie
    };

    public enum Opponent
    {
        AI, Player
    };
}
