using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [SerializeField]
    public string gameVersion = "If you see this, save file error occured =(";
    [SerializeField]
    public List<ObjectData> objects = new List<ObjectData>();

    public GameData()
    {
        objects = new List<ObjectData>();
    }
}
