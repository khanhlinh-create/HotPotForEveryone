using UnityEngine;
using UnityEngine.EventSystems;

public class ItemsSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
    }
}
