using UnityEngine;
using System.Collections;

public class DisplayDelegate : MonoBehaviour {


    public delegate void DisplayEventDelegate(DisplayDelegate displayEvent);
    public DisplayEventDelegate OnAwakeEvent;
    public DisplayEventDelegate OnDestroyEvent;
    public DisplayEventDelegate OnDisableEvent;
    public DisplayEventDelegate OnEnableEvent;

    public object UserData;

    void Awake()
    {
        if(OnAwakeEvent != null)
        {
            OnAwakeEvent(this);
        }
    }

    void OnDestroy()
    {
	    if(OnDestroyEvent != null)
        {
            OnDestroyEvent(this);
        }
	}
	
	void OnDisable()
    {
	    if(OnDisableEvent != null)
        {
            OnDisableEvent(this);
        }

   

    }



    void OnEnable()
    {
        if (OnEnableEvent != null)
        {
            OnEnableEvent(this);
        }
    }

}
