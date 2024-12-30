using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Net.Sockets;
using System.Text;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas;

    AudioManager instance;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    public TextMeshProUGUI yumText;
    public string itemName; // Tên của món ăn
    private ConnectionManager connectionManager;


    private void Start()
    {
        AudioManager.instance.PlayMusic("HotpotSound");
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        connectionManager = FindFirstObjectByType<ConnectionManager>();
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
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        AudioManager.instance.PlaySFX("PickupSound");
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
            AudioManager.instance.PlaySFX("DropSound");
        }
        else
        {
            Debug.Log("Not dropped into ItemsSlot");
            GetComponent<RectTransform>().anchoredPosition = originalPosition;
        }

        if (hoveredObject != null && hoveredObject.GetComponent<Eat>() != null)
        {
            Debug.Log("Dropped into Bowl");
            AudioManager.instance.PlaySFX("EatingSound");

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
        Debug.Log($"Item {itemName} dropped!");
        string gameStateUpdate = $"Item:{itemName}|State:Dropped";
        connectionManager.SendGameStateUpdate(gameStateUpdate); // Gửi tới SubServer
        // Gửi dữ liệu đến SubServer
        Vector2 position = rectTransform.anchoredPosition;
        SendDragDataToSubServer(itemName, position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }

    public void OnDrop(PointerEventData eventData)
    {
        string stateUpdate = $"UpdateState|RoomID|{name}|Dropped";
        connectionManager.SendGameStateUpdate(stateUpdate);

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

    private void SendDragDataToSubServer(string itemName, Vector2 position)
    {
        if (ConnectionManager.subServerClient != null && ConnectionManager.subServerClient.Connected)
        {
            string message = $"Drag|{itemName}|{position.x}|{position.y}";
            NetworkStream stream = ConnectionManager.subServerClient.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log($"Sent drag data to SubServer: {message}");
        }
        else
        {
            Debug.LogError("Not connected to SubServer. Cannot send drag data.");
        }
    }
}
