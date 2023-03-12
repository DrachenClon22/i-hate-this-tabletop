using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEditorInternal;

public class NetObjectsHandler : NetworkBehaviour
{
    private static NetObjectsHandler instance;

    public override void OnNetworkSpawn()
    {
        if (instance)
            Debug.LogWarning("Instance of NetMessageManager is not null.");
        instance = this;
    }

    public static bool isInstance()
    {
        return instance != null;
    }

    public static void SpawnObject(NetworkObject obj_id, bool destroyOnSceneChanged = false)
    {
        instance.SpawnObject_ServerRpc(obj_id.NetworkObjectId, destroyOnSceneChanged);
    }
    public static void SetParent(NetworkObject parent, NetworkObject child)
    {
        instance.SetParent_ServerRpc(parent.NetworkObjectId, child.NetworkObjectId);
    }

    public static void ChangeObjectOwnership(ulong obj_id, ulong user_id, bool ignoreDraggableTag = false, bool resetOwnershipToServer = false)
    {
        instance.ChangeObjectOwnership_ServerRpc(obj_id, user_id, ignoreDraggableTag, resetOwnershipToServer);
    }

    public static void ChangeObjectOwnership(NetworkObject[] obj_id, ulong user_id, bool ignoreDraggableTag = false, bool resetOwnershipToServer = false)
    {
        foreach (NetworkObject obj in obj_id)
        {
            ChangeObjectOwnership(obj.NetworkObjectId, 0, ignoreDraggableTag, resetOwnershipToServer);
        }
    }

    public static void SetObjectLayer(ulong obj_id, string layer)
    {
        instance.SetObjectLayer_ServerRpc(obj_id, layer);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetObjectLayer_ServerRpc(ulong obj_id, string layer)
    {
        SetObjectLayer_ClientRpc(obj_id, layer);
    }

    [ClientRpc]
    private void SetObjectLayer_ClientRpc(ulong obj_id, string layer)
    {
        FindGameobjectWithNetworkId(obj_id).layer = LayerMask.NameToLayer(layer);
    }

    public static GameObject FindGameobjectWithNetworkId(ulong id, bool ignoreDraggableTag = false)
    {
        GameObject[] goes = (ignoreDraggableTag) ? GameObject.FindObjectsOfType<GameObject>() : GameObject.FindGameObjectsWithTag("Draggable");
        foreach (GameObject go in goes)
        {
            if (go.GetComponent<NetworkObject>()?.NetworkObjectId == id)
            {
                return go;
            }
        }

        return null;
    }

    public static void DestroyObject(ulong obj_id)
    {
        instance.DestroyObject_ServerRpc(obj_id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnObject_ServerRpc(ulong obj_id, bool destroyOnSceneChanged = false)
    {
        NetworkObject[] goes = GameObject.FindObjectsOfType<NetworkObject>();
        foreach (NetworkObject go in goes)
        {
            if (go.NetworkObjectId == obj_id)
            {
                go.Spawn(destroyOnSceneChanged);
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetParent_ServerRpc(ulong parent, ulong child)
    {
        if (parent == child)
        {
            Debug.LogError("Cannot set parent to self");
            return;
        }

        NetworkObject[] goes = GameObject.FindObjectsOfType<NetworkObject>();
        Transform local_parent = null;
        Transform local_child = null;
        foreach (NetworkObject go in goes)
        {
            if (go.NetworkObjectId == parent)
            {
                local_parent = go.GetComponent<Transform>();
                continue;
            }

            if (go.NetworkObjectId == child)
            {
                local_child = go.GetComponent<Transform>();
                continue;
            }

            if (local_parent && local_child)
            {
                break;
            }
        }

        if (!(local_parent && local_child))
        {
            Debug.LogError("Cannot find GameObject to set parent");
            return;
        }

        local_parent.SetParent(local_child);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeObjectOwnership_ServerRpc(ulong obj_id, ulong user_id, bool ignoreDraggableTag, bool backToServer)
    {
        //Debug.Log($"User: {user_id} pending ownership {obj_id}");
        GameObject[] goes = (ignoreDraggableTag) ? GameObject.FindObjectsOfType<GameObject>() : GameObject.FindGameObjectsWithTag("Draggable");
        foreach (GameObject go in goes)
        {
            if (go.GetComponent<NetworkObject>()?.NetworkObjectId == obj_id)
            {
                //Debug.Log($"User: {user_id} now owner of {obj_id}");
                Vector3 position = go.transform.position;
                go.GetComponent<NetworkObject>().ChangeOwnership(backToServer ? NetworkManager.Singleton.LocalClientId : user_id);
                go.transform.position = position;
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyObject_ServerRpc(ulong obj_id)
    {
        GameObject go = FindGameobjectWithNetworkId(obj_id);
        if (go.GetComponent<NetworkObject>().IsSpawned)
            go.GetComponent<NetworkObject>().Despawn();
        Destroy(go);
    }
}
