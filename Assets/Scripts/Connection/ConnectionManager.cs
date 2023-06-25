using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Net;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class ConnectionManager : NetworkBehaviour
{
    private static ConnectionManager instance;

    public NetworkManager networkManager;

    private void Start()
    {
        if (instance)
            Destroy(instance.gameObject);
        instance = this;

        if (!NetworkManager.Singleton)
            Instantiate(networkManager);
        
        DontDestroyOnLoad(this);
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        NetworkManager.GetComponent<UnityTransport>().OnTransportEvent -= OnTransportEvent;
        if (!IsOwner) { Destroy(gameObject); }

        if (IsServer) { NetworkManager.OnClientDisconnectCallback += OnClientDisconnect; }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                if (NetworkManager.ConnectedClients.ContainsKey(clientId))
                {
                    NetObjectsHandler.ChangeObjectOwnership(NetworkManager.ConnectedClients[clientId].OwnedObjects.ToArray(), 0, false, true);
                }
            }
        }
    }

    private void OnTransportEvent(NetworkEvent eventType, ulong clientId, System.ArraySegment<byte> payload, float receiveTime)
    {
        if (eventType.Equals(NetworkEvent.Disconnect))
        {
            print(NetworkManager.Singleton.LocalClientId);
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SceneManager.LoadScene("Scenes/MainMenu", LoadSceneMode.Single);
            } else
            {
                Debug.Log($"Client: {clientId} disconnected");
            }
        }

        if (eventType.Equals(NetworkEvent.Connect))
        {
            instance.StartCoroutine(instance.LoadLevel("Scenes/TableLevel"));
        }
    }

    public static void StartClient(string ip, ushort port)
    {
        instance.StartCoroutine(instance.ConnectToServer(ip, port));
    }

    public static void StartHost()
    {
        instance.StartCoroutine(instance.StartHostConnection());
    }

    private IEnumerator StartHostConnection()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("Scenes/TableLevel", LoadSceneMode.Single);
        async.allowSceneActivation = false;
        yield return new WaitWhile(() => async.progress < 0.9f);
        async.allowSceneActivation = true;

        NetworkManager.GetComponent<UnityTransport>().ConnectionData.Address = GetLocalIPv4();
        NetworkManager.GetComponent<UnityTransport>().ConnectionData.Port = 7777;
        NetworkManager.Singleton.GetComponent<UnityTransport>().OnTransportEvent += OnTransportEvent;
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => NetworkManager.StartHost());
    }

    private IEnumerator ConnectToServer(string ip, ushort port)
    {
        NetworkManager.GetComponent<UnityTransport>().ConnectionData.Address = ip;
        NetworkManager.GetComponent<UnityTransport>().ConnectionData.Port = port;
        NetworkManager.Singleton.GetComponent<UnityTransport>().OnTransportEvent += OnTransportEvent;
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => NetworkManager.StartClient());
    }

    private IEnumerator LoadLevel(string level)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(level, LoadSceneMode.Single);
        async.allowSceneActivation = false;
        yield return new WaitWhile(() => async.progress < 0.9f);
        async.allowSceneActivation = true;
    }
}
