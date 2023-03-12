using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectHex : MonoBehaviour
{
    public float speed = 5f;
    public float height = 2f;

    //public RectTransform selectUI;

    [HideInInspector]// Make focux Vector3Int
    public static GameObject focus { get; private set; }

    private bool showUI = false;

    private void Start()
    {
        //selectUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (showUI && focus)
        {
            //ShowUI(focus.transform);
        } else
        {
            //if (selectUI.gameObject.activeSelf)
            //{
            //    showUI = false;
            //    selectUI.gameObject.SetActive(false);
            //}
        }

        if (!focus && !showUI)
            showUI = false;

        HandleRaycast();
    }

    private void HandleRaycast()
    {
        if (Input.GetMouseButtonDown(0) && !CameraController.isMouseOverAnUI())
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.transform.tag.Equals("Cell"))
                {
                    if (focus != hitInfo.transform.gameObject)
                    {
                        focus = hitInfo.transform.gameObject;
                        StartCoroutine(MoveShit(hitInfo.transform));
                    } else
                    {
                        if (!showUI)
                            showUI = true;
                    }
                }
                else
                {
                    if (focus)
                        focus = null;
                }
            }
            else
            {
                if (focus)
                    focus = null;
            }
        }
    }

    private void ShowUI(Transform tr)
    {
        //selectUI.position = RectTransformUtility.WorldToScreenPoint(Camera.main, tr.position);

        //if (selectUI.offsetMax.y < 0)
        //{
        //    selectUI.pivot = new Vector2(0, 0);
        //} else
        //{
        //    selectUI.pivot = new Vector2(0, 0.5f);
        //}

        //if (!selectUI.gameObject.activeSelf)
        //    selectUI.gameObject.SetActive(true);
    }

    private IEnumerator MoveShit(Transform tr)
    {
        Vector3 startPos = tr.localPosition;

        while (tr.localPosition.y < startPos.y + height)
        {
            if (focus != tr.gameObject)
                break;
            tr.localPosition = Vector3.Slerp(tr.localPosition, startPos + Vector3.up * height, Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitWhile(() => focus == tr.gameObject);

        while (tr.localPosition.y > startPos.y)
        {
            tr.localPosition = Vector3.Slerp(tr.localPosition, startPos, Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();
    }
}
