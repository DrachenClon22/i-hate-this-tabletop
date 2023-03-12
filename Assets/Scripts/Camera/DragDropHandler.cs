using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using UnityEngine.Networking;

public class DragDropHandler : NetworkBehaviour
{
    private static Transform draggingObject;

    public float rotationSpeed = 60f;

    private Vector3 tempHiddenPosition;
    private Vector3 clickPositionOffset;

    private RaycastHit hitInfo;

    public override void OnNetworkSpawn()
    {
        gameObject.name = $"Camera_Id:{OwnerClientId}";

        if (!IsOwner) 
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
            enabled = false; 
        }
    }

    public static void SetDraggingObject(Transform obj)
    {
        draggingObject = obj;
    }

    public static Transform getDraggingObject()
    {
        return draggingObject;
    }

    private void Update()
    {
        if (SettingsButton.canMove || IsHost)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!CameraController.isMouseOverAnUI())
                {
                    InitDraggingObject();
                    InitClickOffset();
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (draggingObject)
                {
                    if (!CameraController.isMouseOverAnUI())
                    {
                        HandleMovements();
                        HandleRotation();
                    }
                    else
                    {
                        if (IsHost || SettingsButton.canDelete)
                        {
                            if (DeleteZone.IsInDeleteZone)
                            {
                                tempHiddenPosition = draggingObject.position;
                                draggingObject.position = new Vector3(99999f, 99999f, 99999f);
                            }
                            else { draggingObject.position = tempHiddenPosition; }
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (draggingObject)
                {
                    draggingObject.GetComponent<ObjectInfo>().ChangeBusyState(false);
                    draggingObject.gameObject.layer = LayerMask.NameToLayer("Default");

                    if (DeleteZone.IsInDeleteZone && (IsHost || SettingsButton.canDelete))
                    {
                        NetObjectsHandler.DestroyObject(draggingObject.GetComponent<NetworkObject>().NetworkObjectId);
                    }
                    draggingObject = null;
                }
            }
        }
    }

    private void HandleMovements()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {
            draggingObject.position = newObjectPosition();
        }
    }

    private void HandleRotation()
    {
        if (Input.GetAxis("Rotation") != 0)
        {
            draggingObject.Rotate(Vector3.up * Input.GetAxis("Rotation") * rotationSpeed * Time.deltaTime);
        }
    }

    private void InitDraggingObject()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
        {
            if (hitInfo.collider.transform.CompareTag("Draggable"))
            {
                draggingObject = hitInfo.collider.transform;
                NetObjectsHandler.ChangeObjectOwnership(draggingObject.GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.Singleton.LocalClientId);

                if (!draggingObject.GetComponent<ObjectInfo>().isBusy.Value) { draggingObject.GetComponent<ObjectInfo>().ChangeBusyState(true); }
                else { return; }

                draggingObject.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }

    private void InitClickOffset()
    {
        // Offset.y = object lowest model mesh position
        if (draggingObject)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                clickPositionOffset = hitInfo.point - draggingObject.position;
            }
            else { clickPositionOffset = Vector3.zero; }
        }
    }

    private Vector3 newObjectPosition()
    {
        // to do fix clipping error
        return new Vector3(hitInfo.point.x - clickPositionOffset.x,
                            (draggingObject.GetComponent<Collider>().bounds.size.y / 2) + hitInfo.point.y,
                            hitInfo.point.z - clickPositionOffset.z);
    }
}
