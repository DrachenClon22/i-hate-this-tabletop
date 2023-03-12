using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;

public class SettingsButton : NetworkBehaviour
{
    public NetworkVariable<bool> canPlayerPlaceObject = new NetworkVariable<bool>();
    public NetworkVariable<bool> canPlayerMoveObject = new NetworkVariable<bool>();
    public NetworkVariable<bool> canPlayerDeleteObject = new NetworkVariable<bool>();

    public static bool canPlace = true;
    public static bool canMove = true;
    public static bool canDelete = true;

    public GameObject settingsOverlay;

    public TMPro.TMP_Text m_TextMeshPro;

    public Toggle canPlayerMove_Toggle;
    public Toggle canPlayerPlace_Toggle;
    public Toggle canPlayerDelete_Toggle;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            canPlayerPlaceObject.Value = true;
            canPlayerMoveObject.Value = true;
        }

        canPlayerPlaceObject.OnValueChanged += PlaceValueChanged;
        canPlayerMoveObject.OnValueChanged += MoveValueChanged;
        canPlayerDeleteObject.OnValueChanged += DeleteValueChanged;

        canPlace = canPlayerPlaceObject.Value;
        canMove = canPlayerMoveObject.Value;
        canDelete = canPlayerDeleteObject.Value;
    }

    public void MoveValueChanged(bool previous, bool current)
    {
        canMove = current;
    }

    public void PlaceValueChanged(bool previous, bool current)
    {
        canPlace = current;
    }

    public void DeleteValueChanged(bool previous, bool current)
    {
        canDelete = current;
    }

    public void CanPlayerPlace_Check(bool current)
    {
        if (!canPlayerMove_Toggle.isOn && canPlayerPlace_Toggle.isOn)
        {
            canPlayerMove_Toggle.isOn = true;
            canPlayerMoveObject.Value = true;
        }
        if (IsHost)
            canPlayerPlaceObject.Value = canPlayerPlace_Toggle.isOn;
    }

    public void CanPlayerMove_Check(bool current)
    {
        if (!canPlayerMove_Toggle.isOn && canPlayerPlace_Toggle.isOn)
        {
            canPlayerMove_Toggle.isOn = true;
            canPlayerMoveObject.Value = true;
        }
        if (IsHost)
            canPlayerMoveObject.Value = canPlayerMove_Toggle.isOn;
    }

    public void CanPlayerDelete_Check(bool current)
    {
        if (IsHost)
            canPlayerDeleteObject.Value = current;
    }

    public void ToggleSettingsOverlay_Button(bool toggle)
    {
        settingsOverlay.SetActive(toggle);

        if (toggle)
        {
            string temp = m_TextMeshPro.text;
            m_TextMeshPro.text = temp.Replace("%ip%", NetworkManager.FindObjectOfType<UnityTransport>().ConnectionData.Address);
            m_TextMeshPro.text = m_TextMeshPro.text.Replace("%port%", NetworkManager.FindObjectOfType<UnityTransport>().ConnectionData.Port.ToString());
        }
        if (!IsHost)
        {
            if (canPlayerMove_Toggle.gameObject.activeSelf)
                canPlayerMove_Toggle.gameObject.SetActive(false);
            if (canPlayerPlace_Toggle.gameObject.activeSelf)
                canPlayerPlace_Toggle.gameObject.SetActive(false);
            if (canPlayerDelete_Toggle.gameObject.activeSelf)
                canPlayerDelete_Toggle.gameObject.SetActive(false);
        }
    }
}
