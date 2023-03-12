using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Netcode;
using UnityEngine.Networking;

public class DragMenuItemHandler : NetworkBehaviour, IBeginDragHandler, IPointerDownHandler, IEndDragHandler, IDragHandler
{
    public GameObject prefab;

    [SerializeField]
    private Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private GameObject generatedObject;

    private Vector2 startPosition;

    private bool isPlaced = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (generatedObject)
            generatedObject = null;

        if (!IsHost && SettingsButton.canPlace || IsHost)
        {
            canvasGroup.blocksRaycasts = false;
            startPosition = rectTransform.anchoredPosition;

            SpawnPrefab_ServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPrefab_ServerRpc(ulong user_id)
    {
        generatedObject = Instantiate(prefab, new Vector3(99999f, 99999f, 99999f), Quaternion.identity);
        generatedObject.GetComponent<NetworkObject>().SpawnWithOwnership(user_id);

        ManagePrefab_ClientRpc(generatedObject.GetComponent<NetworkObject>().NetworkObjectId, user_id);
    }

    [ClientRpc]
    private void ManagePrefab_ClientRpc(ulong result, ulong user_id)
    {
        if (generatedObject = NetObjectsHandler.FindGameobjectWithNetworkId(result))
        {
            ManageObject(user_id);
        }
        else
        {
            Debug.LogWarning($"Can't find spawned object. ObjectID: {result}; Target User(s): {NetworkManager.Singleton.LocalClientId}");
        }
    }

    private void ManageObject(ulong user_id)
    {
        if (generatedObject.transform.childCount > 0)
        {
            // Make objects layer Default, but make em busy, but what if children doesn't have ObjectInfo?
            foreach (Transform go in generatedObject.transform.GetComponentsInChildren<Transform>())
            {
                go.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        } else { generatedObject.layer = LayerMask.NameToLayer("Ignore Raycast"); }

        if (IsHost) { generatedObject.GetComponent<ObjectInfo>().Init(prefab.GetComponent<ObjectInfo>().prefabPath, user_id); }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (generatedObject)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

            if (!CameraController.isMouseOverAnUI())
            {
                if (canvasGroup.alpha != 0f)
                    canvasGroup.alpha = 0f;
                HandleRaycast();

            }
            else
            {
                generatedObject.transform.position = new Vector3(99999f, 99999f, 99999f);
                if (isPlaced)
                    isPlaced = false;
                if (canvasGroup.alpha != 0.8f)
                    canvasGroup.alpha = 0.8f;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (generatedObject)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = startPosition;

            if (!isPlaced)
                NetObjectsHandler.DestroyObject(generatedObject.GetComponent<NetworkObject>().NetworkObjectId);
            else
                NetObjectsHandler.SetObjectLayer(generatedObject.GetComponent<NetworkObject>().NetworkObjectId, "Default");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // show hint
        // OnPointerUp destroy hint
    }

    private void HandleRaycast()
    {
        if (!CameraController.isMouseOverAnUI())
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition)))
            {
                if (DragDropHandler.getDraggingObject() != generatedObject.transform)
                    DragDropHandler.SetDraggingObject(generatedObject.transform);
                if (!isPlaced)
                    isPlaced = true;
            }
        }
    }
}
