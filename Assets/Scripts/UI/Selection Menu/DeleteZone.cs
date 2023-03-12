using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsInDeleteZone { get; private set; } = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsInDeleteZone = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsInDeleteZone = false;
    }
}
