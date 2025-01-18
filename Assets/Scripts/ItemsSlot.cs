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
        if (parts.Length == 3 && parts[0] == "UpdateState")
        {
            string itemName = parts[1]; // Tên của topping
            string position = parts[2]; // Vị trí dưới dạng "x,y,z"

            // Tìm item dựa trên tên và cập nhật vị trí
            Transform toppingsParent = GameObject.Find("Canvas/Toppings")?.transform;
            if (toppingsParent == null)
            {
                Debug.LogError("Parent object 'Canvas/Toppings' not found.");
                return;
            }

            Transform itemTransform = toppingsParent.Find(itemName);
            if (itemTransform != null)
            {
                // Parse vị trí từ dữ liệu nhận được
                string[] posParts = position.Split(',');
                if (posParts.Length == 3 &&
                    float.TryParse(posParts[0], out float x) &&
                    float.TryParse(posParts[1], out float y) &&
                    float.TryParse(posParts[2], out float z))
                {
                    Vector3 newPosition = new Vector3(x, y, z);
                    itemTransform.position = newPosition;
                    Debug.Log($"Updated {itemName} position to: {newPosition}");
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


}
