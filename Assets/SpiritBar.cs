using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpiritBar : MonoBehaviour
{
    public Image spiritBarImage;

    public int min;
    public int max;

    private int mCurrentValue;
    private float mCurrentPercent;

    public void SetSpirit(int spirit) {
        if (max - min == 0) {
            mCurrentValue = 0;
            mCurrentPercent = 0;
        } else {
            mCurrentValue = spirit;
            mCurrentPercent = (float)mCurrentValue / (float)(max - min);
        }

        spiritBarImage.fillAmount = mCurrentPercent;
    }
}
