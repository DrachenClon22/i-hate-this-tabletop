using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(ClientNetworkTransform))]
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class ObjectInfo : NetworkBehaviour
{
    public NetworkVariable<bool> isBusy = new NetworkVariable<bool>(false);
    public NetworkVariable<ulong> userIdPlaced = new NetworkVariable<ulong>();

    public GameObject prefab;
    public string prefabPath;

    public override void OnNetworkSpawn()
    {
        gameObject.name = $"{gameObject.name.Replace("(Clone)","")}_ID:{userIdPlaced.Value}";
    }

    public void Init(string prefabPath, ulong userId)
    {
        this.prefabPath = prefabPath;
        this.userIdPlaced.Value = userId;
    }

    private void Start()
    {
        if (!gameObject.tag.Equals("Draggable"))
        {
            Debug.LogWarning("Object is supposed to be draggable, but tag is not.");
            gameObject.tag = "Draggable";
        }
    }

    public void ChangeBusyState(bool state)
    {
        ChangeBusyStateServerRpc(state);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeBusyStateServerRpc(bool state)
    {
        isBusy.Value = state;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        prefabPath = AssetDatabase.GetAssetPath(prefab).Replace("Assets/Resources/", "").Replace(".prefab", "");
    }
#endif
}
