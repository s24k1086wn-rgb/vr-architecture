using UnityEngine;

public class CubeClick : MonoBehaviour
{
    private bool timerRunning = false;
    private float startTime;

    void OnMouseDown()
    {
        if (!timerRunning)
        {
            startTime = Time.time;
            timerRunning = true;

            Debug.Log("タイマー開始");
        }
        else
        {
            float elapsedTime = Time.time - startTime;
            timerRunning = false;

            Debug.Log("タイマー停止");
            Debug.Log("経過時間: " + elapsedTime + "秒");
        }
    }
}
