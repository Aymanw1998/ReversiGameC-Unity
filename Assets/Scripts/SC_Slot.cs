using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Slot : MonoBehaviour
{
    public Image curImg;
    public Sprite blackCircle, whiteCircle;
    // Start is called before the first frame update

    public void ChangeSlotState(GlobalEnums.SloState _CurSlot)
    {
        if (curImg == null)
        {
            Debug.LogError("cur Img is Null, pass reference");
        }
        else
        {
            switch (_CurSlot)
            {
                case GlobalEnums.SloState.Empty:
                    curImg.enabled = false; break;
                case GlobalEnums.SloState.Black:
                    curImg.sprite = blackCircle;
                    curImg.enabled = true;
                    break;
                case GlobalEnums.SloState.White:
                    curImg.sprite = whiteCircle;
                    curImg.enabled = true; break;
            }
        }

    }
}
