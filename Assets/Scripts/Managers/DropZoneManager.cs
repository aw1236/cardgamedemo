using UnityEngine;
using System.Collections.Generic;

public class DropZoneManager : MonoBehaviour
{
    [Header("卡槽放置区域")]
    public List<RectTransform> slotDropZones = new List<RectTransform>();  // 所有卡槽的检测区域

    public static DropZoneManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 检查位置是否在任何卡槽区域内
    public bool IsInAnySlotZone(Vector2 screenPosition)
    {
        if (slotDropZones.Count == 0)
        {
            Debug.LogWarning("没有设置任何卡槽检测区域！");
            return true; // 没有设置区域时允许放置
        }

        foreach (RectTransform dropZone in slotDropZones)
        {
            if (dropZone == null) continue;

            Vector2 localPoint;
            bool inRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dropZone,
                screenPosition,
                null,
                out localPoint
            );

            if (inRect && dropZone.rect.Contains(localPoint))
            {
                return true;
            }
        }

        return false;
    }
}