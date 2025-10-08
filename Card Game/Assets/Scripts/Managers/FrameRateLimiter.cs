using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;

    void Start()
    {
        // 设置目标帧率
        Application.targetFrameRate = targetFrameRate;

        // 可选：关闭垂直同步以获得更精确的帧率控制
        // QualitySettings.vSyncCount = 0;
    }

    // 运行时动态修改帧率
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