﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    public TextMeshProUGUI yumText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        var hoveredObject = eventData.pointerEnter;
        if (hoveredObject != null && hoveredObject.GetComponent<ItemsSlot>() != null)
        {
            Debug.Log("Dropped into ItemsSlot");
            StartCoroutine(DisableDragTemporarily(5f));
        }
        else
        {
            Debug.Log("Not dropped into ItemsSlot");
            GetComponent<RectTransform>().anchoredPosition = originalPosition;
        }

        if (hoveredObject != null && hoveredObject.GetComponent<Eat>() != null)
        {
            Debug.Log("Dropped into Bowl");

            /*if (yumText != null)
            {
                StartCoroutine(DisplayYumText());
            }*/

            eventData.pointerDrag.SetActive(false);
        }
        else
        {
            Debug.Log("Not dropped into Bowl");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }

    public void OnDrop(PointerEventData eventData)
    {

    }

    private IEnumerator DisableDragTemporarily(float delay)
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        var graphic = GetComponent<UnityEngine.UI.Graphic>();
        if (graphic == null)
        {
            Debug.LogError("Graphic component not found!");
            yield break;
        }

        Color originalColor = graphic.color;
        Color targetColor = originalColor * 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            graphic.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }
        graphic.color = targetColor;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
        Debug.Log("Dragging disabled");

        yield return new WaitForSeconds(delay);

        fadeDuration = 1f;
        elapsedTime = 0f;
        ColorUtility.TryParseHtmlString("#CDA294", out Color parsedColor);
        Color resetTargetColor = originalColor * parsedColor;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            graphic.color = Color.Lerp(graphic.color, resetTargetColor, t);
            yield return null;
        }
        graphic.color = resetTargetColor;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
        Debug.Log("Dragging enabled");
    }

    private IEnumerator DisplayYumText()
    {
        yumText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        yumText.gameObject.SetActive(false);
    }
}
