using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
using System;

public class MapDataManager : NetworkBehaviour, IDataManager
{
    // Make in game choice
    public bool supportOldVersionSaves = true;

    public void LoadData(GameData data)
    {
        if (IsServer)
        {
            if (data.gameVersion != Application.version)
            {
                // Make warning here
                if (!supportOldVersionSaves)
                {
                    Debug.LogWarning("Save file version mismatch. Enable ignoring saves mismatch or just don't try to load them due to save file corruption chance.");
                    Debug.LogWarning($"Current game version: {Application.version} || Save game version: {data.gameVersion}");
                    return;
                }
            }

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Draggable"))
            {
                if (go.GetComponent<NetworkObject>())
                {
                    NetObjectsHandler.DestroyObject(go.GetComponent<NetworkObject>().NetworkObjectId);
                }
            }
            
            try
            {
                foreach (ObjectData od in data.objects)
                {
                    GameObject fromPrefab = Resources.Load<GameObject>(od.prefab);
                    if (fromPrefab)
                    {
                        GameObject go = Instantiate(fromPrefab, od.pos, od.rot);
                        ObjectInfo objInfo = (go.GetComponent<ObjectInfo>()) ? go.GetComponent<ObjectInfo>() : go.AddComponent<ObjectInfo>();
                        objInfo.prefab = fromPrefab;
                        go.tag = "Draggable";
                        go.GetComponent<NetworkObject>().Spawn();
                    }
                }
#if UNITY_EDITOR
                Debug.Log("Loaded!");
#endif
            } catch (Exception e)
            {
                Debug.LogError($"Error occured when loading the save.\n{e}");
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (IsServer)
        {
            data.objects = null;
            data.objects = GetAllObjectsOnTheScene();
            data.gameVersion = Application.version;
#if UNITY_EDITOR
            Debug.Log("Saved!");
#endif
        }
    }

    private List<ObjectData> GetAllObjectsOnTheScene()
    {
        List<ObjectData> objects = new List<ObjectData>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Draggable"))
        {
            if (go.GetComponent<ObjectInfo>())
            {
                objects.Add(new ObjectData(go.GetComponent<ObjectInfo>().prefabPath, go.transform.position, go.transform.rotation));
            }
        }
        return objects;
    }
}
