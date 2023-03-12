using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragDropFromMenu : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler, IEndDragHandler
{
    [SerializeField]
    private Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private GameObject focus;
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
        canvasGroup.blocksRaycasts = false;
        startPosition = rectTransform.anchoredPosition;

        //generatedObject = Instantiate(gridObject.prefab, new Vector3(99999f, 99999f, 99999f), Quaternion.identity);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (!CameraController.isMouseOverAnUI())
        {
            if (canvasGroup.alpha != 0f)
                canvasGroup.alpha = 0f;
            HandleRaycast();
        } else
        {
            if (focus)
                focus = null;
            if (generatedObject)
                generatedObject.transform.position = new Vector3(99999f, 99999f, 99999f);
            if (isPlaced)
                isPlaced = false;
            if (canvasGroup.alpha != 0.8f)
                canvasGroup.alpha = 0.8f;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = startPosition;

        if (!isPlaced && generatedObject)
            Destroy(generatedObject);
        else
        {
            focus.GetComponent<Cell>().something_placed = true;
            generatedObject.transform.SetParent(focus.transform);
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
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.transform.tag.Equals("Cell"))
                {
                    if (focus != hitInfo.transform.gameObject)
                    {
                        focus = hitInfo.transform.gameObject;
                        ManagePlacementObject();
                        isPlaced = true;
                    }
                }
                else
                {
                    if (focus)
                        focus = null;
                }
            }
        }
    }

    private void ManagePlacementObject()
    {
        if (focus && generatedObject)
        {
            generatedObject.transform.position = focus.GetComponent<Cell>().worldPosition;
        }
    }
}
