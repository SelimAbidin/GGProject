using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StructureButtonCounter : MonoBehaviour
{
    public GameObject counterText;
    public string preFix;
    public string postFix;
    public string refStructureName;
    public int maxCount;
    public bool isLimited = true;
    int counter;
    public void SetCountText(int count)
    {
        counter = count;
        if (counterText  != null )
        {
            if(counter >= 0)
            {
                isLimited = true;
                counterText.SetActive(true);
                counterText.GetComponent<Text>().text = preFix + count + postFix;
            }
            else
            {
                isLimited = false;
                counterText.SetActive(false);
            }
        }
        
        
    }
}
