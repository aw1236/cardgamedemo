using UnityEngine;
using UnityEngine.EventSystems;

public class SlotDebugger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("🟢 鼠标进入主角槽范围");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("🔴 鼠标离开主角槽范围");
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("🎯 有卡牌拖放到主角槽！");

        if (eventData.pointerDrag != null)
        {
            Debug.Log($"拖拽对象: {eventData.pointerDrag.name}");

            CardView cardView = eventData.pointerDrag.GetComponent<CardView>();
            if (cardView != null)
            {
                Debug.Log($"卡牌名称: {cardView.GetCardData()?.cardName}");
            }
        }
    }
}