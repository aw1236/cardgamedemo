using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;

    void Start()
    {
        // ����Ŀ��֡��
        Application.targetFrameRate = targetFrameRate;

        // ��ѡ���رմ�ֱͬ���Ի�ø���ȷ��֡�ʿ���
        // QualitySettings.vSyncCount = 0;
    }

    // ����ʱ��̬�޸�֡��
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Application.targetFrameRate = 30;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Application.targetFrameRate = 60;
        }
    }
}