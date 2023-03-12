using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ObjectData
{
    [SerializeField]
    public string prefab;
    [SerializeField]
    public Vector3 pos;
    [SerializeField]
    public Quaternion rot;

    public ObjectData(string path, Vector3 pos, Quaternion rot)
    {
        this.prefab = path;
        this.pos = pos;
        this.rot = rot;
    }
}
