using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.UI;

public class LoadSaveButtonsActivation : NetworkBehaviour
{
    public Button loadGame;
    public Button saveGame;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            loadGame.interactable = false;
            saveGame.interactable = false;
        }
    }
}
