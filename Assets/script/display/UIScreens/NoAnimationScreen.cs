using UnityEngine;
using System.Collections;
using System;

public class NoAnimationScreen : UIScreen
{

    public override void EnterScene()
    {
        this.gameObject.SetActive(true);
        if (onScreenEnterComplete != null)
        {
            onScreenEnterComplete(this);
        }
    }

    public override void LeaveScene()
    {
        this.gameObject.SetActive(false);
        if (onScreenLeaveComplete != null)
        {
            onScreenLeaveComplete(this);
        }
    }


}
