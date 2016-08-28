using UnityEngine;
using System.Collections;

public class CameraYDragControl : MonoBehaviour {

    private Vector3 dragOrigin;
    private bool isDown = false;
	void Update ()
    {

        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isDown = true;
            dragOrigin = Input.mousePosition; 
            return;
        }


        if (Input.GetMouseButtonUp(0))
        {
            isDown = false;
            return;
        }

        if (!Input.GetMouseButton(0)) return;


      
         if(isDown)
        {
            Vector3 pos = Input.mousePosition;
            Vector3 difference = ((dragOrigin - pos) * (Camera.main.orthographicSize / Screen.height)) * 2f;
            transform.position = new Vector3(transform.position.x, transform.position.y + difference.y, transform.position.z);
            dragOrigin = pos;
        }
      
    }
}
