using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public TMPro.TMP_InputField ip;
    public TMPro.TMP_InputField port;
    public TMPro.TextMeshProUGUI underline;

    public void StartServer_Button()
    {
        ConnectionManager.StartHost();
    }

    public void ConnectToServer_Button(Button button)
    {
        underline.text = "";
        ConnectionManager.StartClient(ip.text, ushort.Parse(port.text));
    }
}
