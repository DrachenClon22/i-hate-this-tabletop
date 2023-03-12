using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.Netcode;

// TODO
// If needed add follow stuff

public class CameraController : NetworkBehaviour
{
    public float movementSpeed = 1f;
    public float groundChangeSpeed = 4f;
    public float movementTime = 5f;
    public float movementMultiplier = 3f;
    public float rotationAmount = 0.7f;
    public float maxHeight = 40f;
    [Space(5)]
    public Vector2 cameraDistanceToPoint = new Vector2(10f, 400f);
    public Vector2 cameraAngleRestrictions = new Vector2(10f, 80f);
    [Space(5)]
    public bool invertMouseX = false;
    public bool invertMouseY = true;

    public Vector3 zoomAmount = new Vector3(0, 0f, -1f);
    [Space(5)]
    public GameObject playerHead;
    public GameObject playerCamera;

    // Temp Variables
    private Vector3 newPosition;
    private Vector3 newZoom;
    private Quaternion newRotation;
    private float temp_movementSpeed;
    // Mouse Inputs
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private Vector3 rotateStartPosition;
    private Vector3 rotateCurrentPosition;
    // Usually just Child of the object
    private Transform cameraTransform;

    private RaycastHit hitInfo;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        StartCoroutine(init());
    }

    private IEnumerator init()
    {
        yield return new WaitUntil(()=>NetObjectsHandler.isInstance());

        if (IsOwner)
        {
            InitCamera_ServerRpc(NetworkManager.Singleton.LocalClientId);
            yield return new WaitUntil(() => cameraTransform);

            gameObject.name = $"PLAYER_CURRENT";
            newPosition = transform.position;
            newRotation = transform.rotation;
            newZoom = cameraTransform.localPosition;
        }
        else
        {
            gameObject.name = $"PLAYER_OTHER";
            enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitCamera_ServerRpc(ulong user_id)
    {
        GameObject head = Instantiate(playerHead, new Vector3(99999f, 99999f, 99999f), Quaternion.identity);
        GameObject camera = Instantiate(playerCamera, new Vector3(99999f, 99999f, 99999f), Quaternion.identity);

        Transform loc_cameraTransform = camera.transform;

        loc_cameraTransform.gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(user_id);
        head.GetComponent<NetworkObject>().SpawnWithOwnership(user_id);

        loc_cameraTransform.SetParent(transform);
        head.transform.SetParent(loc_cameraTransform);

        ManageCamera_ClientRpc(loc_cameraTransform.gameObject.GetComponent<NetworkObject>().NetworkObjectId,
            head.GetComponent<NetworkObject>().NetworkObjectId,
            user_id);
    }

    [ClientRpc]
    private void ManageCamera_ClientRpc(ulong camera, ulong head, ulong user_id)
    {
        if (user_id==NetworkManager.Singleton.LocalClientId)
        {
            cameraTransform = NetObjectsHandler.FindGameobjectWithNetworkId(camera, true)?.transform;
            Transform _head = NetObjectsHandler.FindGameobjectWithNetworkId(head, true)?.transform;

            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = Quaternion.identity;

            _head.localPosition = Vector3.zero;
            _head.localRotation = Quaternion.identity;
        }
    }

    private void FixedUpdate()
    {
        if (cameraTransform && IsOwner)
        {
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.fixedDeltaTime * movementTime);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
            newRotation = Quaternion.Euler(Mathf.Clamp(newRotation.eulerAngles.x, cameraAngleRestrictions.x, cameraAngleRestrictions.y),
                newRotation.eulerAngles.y, 0f);

            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.fixedDeltaTime * movementTime);
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.fixedDeltaTime * movementTime);
        }
    }

    private void Update()
    {
        if (cameraTransform && IsOwner)
        {
            if (!isMouseOverAnUI())
            {
                HandleMouseInput();
            }
            HandleMovement();
            HandleRotation();
            HandleZoom();
            HandleGroundCurves();

            temp_movementSpeed = movementSpeed * (Vector3.Distance(transform.position, cameraTransform.position) / cameraDistanceToPoint.y);
        }
    }

    private void HandleGroundCurves()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward * cameraDistanceToPoint.y * 2f, out hitInfo))
        {
            if (!hitInfo.collider.CompareTag("Draggable"))
            {
                newPosition.y = Mathf.Lerp(newPosition.y, hitInfo.point.y, Time.deltaTime * groundChangeSpeed);
                newPosition.y = Mathf.Clamp(newPosition.y, -maxHeight, maxHeight);
            }
        }
    }

    private void HandleMouseInput()
    {
        // Scroll event
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom -= Input.mouseScrollDelta.y * (Vector3.Distance(transform.position, cameraTransform.position) / 10f)
                * zoomAmount * calculateFastCameraMultiplier();
            newZoom.y = Mathf.Clamp(newZoom.y, cameraDistanceToPoint.x, cameraDistanceToPoint.y);
            newZoom.z = Mathf.Clamp(newZoom.z, -cameraDistanceToPoint.y, -cameraDistanceToPoint.x);
        }

        // Right mouse button event
        if (Input.GetMouseButtonDown(1))
        {
            rotateStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x/5f) * ((invertMouseX) ? -1 : 1));
            newRotation *= Quaternion.Euler(Vector3.right * (-difference.y/5f) * ((invertMouseY) ? -1 : 1));
        }

        // Middle mouse button event
        if (Input.GetMouseButtonDown(2))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry = 0f;
            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(2))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry = 0f;
            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
    }

    public static bool isMouseOverAnUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    private void HandleMovement()
    {
        if (PlayerController.isDragging)
        {
            float multX = (2f * Input.mousePosition.x) / Screen.width - 1;
            float multY = (2f * Input.mousePosition.y) / Screen.height - 1;
            if (Mathf.Abs(multX) > 0.75f)
            {
                newPosition += new Vector3(transform.right.x, 0f, transform.right.z) * temp_movementSpeed * multX * calculateFastCameraMultiplier();
            }

            if (Mathf.Abs(multY) > 0.75f)
            {
                newPosition += new Vector3(transform.forward.x, 0f, transform.forward.z) * temp_movementSpeed * multY * calculateFastCameraMultiplier();
            }
        }

        if (Input.GetAxis("Vertical") != 0)
        {
            newPosition += new Vector3(transform.forward.x, 0f, transform.forward.z) * temp_movementSpeed * Input.GetAxis("Vertical") * calculateFastCameraMultiplier();
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            newPosition += new Vector3(transform.right.x, 0f, transform.right.z) * temp_movementSpeed * Input.GetAxis("Horizontal") * calculateFastCameraMultiplier();
        }
    }

    private void HandleRotation()
    {
        if (Input.GetAxis("Rotation") != 0)
        {
            if (!DragDropHandler.getDraggingObject())
            {
                newRotation *= Quaternion.Euler(Vector3.up * rotationAmount * Input.GetAxis("Rotation"));
            }
        }
    }

    private void HandleZoom()
    {
        if (Input.GetAxis("Zoom") != 0)
        {
            //newZoom += zoomAmount * Input.GetAxis("Zoom");
        }
    }

    private float calculateFastCameraMultiplier()
    {
        return (Input.GetAxis("Sprint") != 0) ? (Input.GetAxis("Sprint") * movementMultiplier) : 1f;
    }
}
