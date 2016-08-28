using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class SaveLoadManager : MonoBehaviour {

    private static List<SavableComponent> saveableObjects = new List<SavableComponent>();

    private static string SAVE_FILE_NAME = "savefile.bin";
    public static void Save(SavableComponent savableGame)
    {

        saveableObjects.Add(savableGame);
        Save();
    }

    private static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + SAVE_FILE_NAME); 
        bf.Serialize(file, saveableObjects);
        file.Close();
    }


    public static List<SavableComponent> Load()
    {
        if (File.Exists(Application.persistentDataPath + "/" + SAVE_FILE_NAME))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + SAVE_FILE_NAME, FileMode.Open);
            saveableObjects = (List<SavableComponent>)bf.Deserialize(file);
            file.Close();
            return saveableObjects;
        }

        return null;
    }

    internal static void Reset()
    {
        saveableObjects.Clear();
        Save();
    }
}
