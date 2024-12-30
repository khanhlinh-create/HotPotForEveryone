using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using System.Net.Sockets;

public class Eat: MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public void EatItem(string itemName)
    {
        Debug.Log($"Eating item: {itemName}");
        SendEatDataToSubServer(itemName);
    }

    private void SendEatDataToSubServer(string itemName)
    {
        if (ConnectionManager.subServerClient != null && ConnectionManager.subServerClient.Connected)
        {
            string message = $"Eat|{itemName}";
            NetworkStream stream = ConnectionManager.subServerClient.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log($"Sent eat data to SubServer: {message}");
        }
        else
        {
            Debug.LogError("Not connected to SubServer. Cannot send eat data.");
        }
    }
}
