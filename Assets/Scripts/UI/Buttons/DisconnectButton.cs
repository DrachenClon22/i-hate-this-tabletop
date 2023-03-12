using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities;
using System.Linq;

public class DisconnectButton : NetworkBehaviour
{
    public void Disconnect_Button()
    {
        if (IsHost)
        {
            NetworkClient[] clients = NetworkManager.ConnectedClientsList.ToArray();
            foreach (NetworkClient client in clients)
            {
                if (client.ClientId != NetworkManager.Singleton.LocalClientId)
                {
                    if (client.OwnedObjects.Count > 0)
                    {
                        NetObjectsHandler.ChangeObjectOwnership(client.OwnedObjects.ToArray(), 0, false, true);
                    }
                    ClientRpcParams clientRpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { client.ClientId }
                        }
                    };
                    DisconnectClientRpc(clientRpcParams);
                }
            }
        } else
        {
            if (NetworkManager.Singleton.LocalClient.OwnedObjects.Count > 0)
            {
                NetObjectsHandler.ChangeObjectOwnership(NetworkManager.Singleton.LocalClient.OwnedObjects.ToArray(), 0, false, true);
            }
        }
        StartCoroutine(BeginExit());
    }

    [ClientRpc]
    private void DisconnectClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(BeginExit());
    }

    private IEnumerator BeginExit()
    {
        yield return new WaitForSeconds(0.5f);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Scenes/MainMenu", LoadSceneMode.Single);
    }
}
