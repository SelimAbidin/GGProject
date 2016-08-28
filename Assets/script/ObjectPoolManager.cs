using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectPoolManager : MonoBehaviour {

    public static ObjectPoolManager instance;
    private static Dictionary<string, List<GameObject>> deactiveList = new Dictionary<string, List<GameObject>>();
    private static Dictionary<string, List<GameObject>> aliveList = new Dictionary<string, List<GameObject>>();

    void Awake ()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
           Destroy(gameObject);
        
    }
	
    private static GameObject GetNext(GameObject gameObject)
    {
        GameObject _tempGameObject;
        if (!deactiveList.ContainsKey(gameObject.name))
        {
            deactiveList.Add(gameObject.name , new List<GameObject>());
        }
        else
        {
            List<GameObject> glist =  deactiveList[gameObject.name];


            if (glist.Count > 0)
            {
                _tempGameObject = deactiveList[gameObject.name][0];
                deactiveList[gameObject.name].Remove(_tempGameObject);
                _tempGameObject.SetActive(true);
                return _tempGameObject;
            }
           
        }
        _tempGameObject = GameObject.Instantiate(gameObject);
        _tempGameObject.name = gameObject.name;
        return _tempGameObject;
    }
    
    public static GameObject Instantiate(GameObject cloneObject)
    {
        GameObject gameObject = GetNext(cloneObject);
        if(!aliveList.ContainsKey(gameObject.name))
        {
            
            aliveList.Add(gameObject.name, new List<GameObject>());
        }


        if(!aliveList[gameObject.name].Contains(gameObject))
            aliveList[gameObject.name].Add(gameObject);


        IResetable[] resetComps = gameObject.GetComponents<IResetable>();
        for (int i = 0; i < resetComps.Length; i++)
        {
            resetComps[i].OnInstantiate();
        }

        return gameObject;
    }

    public static void Destroy(GameObject cloneObject)
    {
        cloneObject.SetActive(false);
        IResetable[] resetComps = cloneObject.GetComponents<IResetable>();
        for (int i = 0; i < resetComps.Length; i++)
        {
            resetComps[i].OnDestroy();
        }

        if (aliveList.ContainsKey(cloneObject.name))
        {
            aliveList[cloneObject.name].Remove(cloneObject);
        }

        if(!deactiveList[cloneObject.name].Contains(cloneObject)) deactiveList[cloneObject.name].Add(cloneObject);

    }

    public static string GetRefName(GameObject gameObject)
    {
        return gameObject.name;
    }

    public static void Destroy(List<GameObject> cloneObjects)
    {
        for (int i = 0; i < cloneObjects.Count; i++)
        {
            Destroy(cloneObjects[i]);
        }
    }


    public static int getActiveCountsByName(string cloneObjectRef)
    {
        if(aliveList.ContainsKey(cloneObjectRef))
        {
            return aliveList[cloneObjectRef].Count;
        }
        return 0;
    }

    public static int getDeactiveCountsByName(string cloneObjectRef)
    {
        if (deactiveList.ContainsKey(cloneObjectRef))
        {
            return deactiveList[cloneObjectRef].Count;
        }
        return 0;
    }

    internal static void DestroyAllByRefName(string refName)
    {
        if(aliveList.ContainsKey(refName))
        {
            List<GameObject> list = aliveList[refName];

            while (list.Count > 0)
            {
                Destroy(list[0]);
            }
        }
    }



    public interface IResetable
    {
        void OnInstantiate();
        void OnDestroy();
    }


}
