using UnityEngine;
using System.Collections.Generic;

public class DropZoneManager : MonoBehaviour
{
    [Header("���۷�������")]
    public List<RectTransform> slotDropZones = new List<RectTransform>();  // ���п��۵ļ������

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

    // ���λ���Ƿ����κο���������
    public bool IsInAnySlotZone(Vector2 screenPosition)
    {
        if (slotDropZones.Count == 0)
        {
            Debug.LogWarning("û�������κο��ۼ������");
            return true; // û����������ʱ�������
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