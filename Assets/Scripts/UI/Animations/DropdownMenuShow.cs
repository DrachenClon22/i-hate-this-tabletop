using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownMenuShow : MonoBehaviour
{
    public static bool isOpen { get; private set; }

    public float fadeSpeed = 400f;

    private RectTransform rectTransform;

    private Coroutine ienum;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        isOpen = !(rectTransform.anchoredPosition.x > 0f);
#if !UNITY_EDITOR
        if (rectTransform.anchoredPosition.x < rectTransform.rect.width)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.rect.width, rectTransform.anchoredPosition.y);
        }
#endif
    }

    public void b_show_toggle()
    {
        if (ienum != null)
            StopCoroutine(ienum);
        ienum = StartCoroutine(MoveShit());
    }

    private IEnumerator MoveShit()
    {
        if (rectTransform.anchoredPosition.x > 0f)
        {
            isOpen = true;
            while (rectTransform.anchoredPosition.x > 0f)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x - 
                    fadeSpeed * Time.deltaTime, rectTransform.anchoredPosition.y);
                yield return new WaitForEndOfFrame();
                
            }
        } else
        {
            isOpen = false;
            while (rectTransform.anchoredPosition.x < rectTransform.rect.width)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x +
                    fadeSpeed * Time.deltaTime, rectTransform.anchoredPosition.y);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
