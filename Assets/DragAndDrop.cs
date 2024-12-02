using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
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
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        //StartCoroutine(DisableDragTemporarily(5f));
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
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        Debug.Log("Dragging disabled");

        yield return new WaitForSeconds(delay);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        Debug.Log("Dragging enabled");
    }
}
