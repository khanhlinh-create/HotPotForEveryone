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
        if (parts[0] == "UpdateState")
        {
            string itemName = parts[2];
            string position = parts[3];
            Debug.Log($"Slot update received for {itemName} at position: {position}");

            // Cập nhật giao diện
            UpdateItemPosition(itemName, position);
        }
    }

    // Thêm phương thức này để cập nhật giao diện
    private void UpdateItemPosition(string itemName, string position)
    {
        // Tìm đối tượng item dựa trên tên trong hierarchy của "Canvas/Toppings"
        Transform toppingsParent = GameObject.Find("Canvas/Toppings")?.transform;
        if (toppingsParent == null)
        {
            Debug.LogError("Parent object 'Canvas/Toppings' not found.");
            return;
        }

        // Tìm item trong các con của Toppings
        Transform itemTransform = toppingsParent.Find(itemName);
        if (itemTransform != null)
        {
            // Parse vị trí thành Vector3
            string[] posParts = position.Split(',');
            if (posParts.Length == 3 &&
                float.TryParse(posParts[0], out float x) &&
                float.TryParse(posParts[1], out float y) &&
                float.TryParse(posParts[2], out float z))
            {
                Vector3 newPosition = new Vector3(x, y, z);
                itemTransform.position = newPosition;
                Debug.Log($"Item {itemName} moved to position: {newPosition}");
            }
            else
            {
                Debug.LogError($"Invalid position format: {position}");
            }
        }
        else
        {
            Debug.LogError($"Item '{itemName}' not found under 'Canvas/Toppings'.");
        }
    }

}
