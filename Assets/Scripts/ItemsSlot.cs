using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using System.Net.Sockets;

public class ItemsSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
    }

    public void UpdateSlotState(string itemName, string state)
    {
        Debug.Log($"Updating slot for {itemName} with state: {state}");
        SendSlotDataToSubServer(itemName, state);
    }

    private void SendSlotDataToSubServer(string itemName, string state)
    {
        if (ConnectionManager.subServerClient != null && ConnectionManager.subServerClient.Connected)
        {
            string message = $"UpdateSlot|{itemName}|{state}";
            NetworkStream stream = ConnectionManager.subServerClient.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log($"Sent slot update to SubServer: {message}");
        }
        else
        {
            Debug.LogError("Not connected to SubServer. Cannot send slot update.");
        }
    }

    public void HandleSlotUpdate(string data)
    {
        string[] parts = data.Split('|');
        if (parts.Length >= 3 && parts[0] == "UpdateSlot")
        {
            string itemName = parts[1];
            string state = parts[2];
            Debug.Log($"Slot update received for {itemName}: {state}");
            // C?p nh?t tr?ng thái lên UI n?u c?n
        }
    }
}
