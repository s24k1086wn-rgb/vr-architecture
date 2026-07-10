using UnityEngine;

public class CraneRotate : MonoBehaviour
{
    public float startDelay = 60f;      // 教習開始から60秒後
    public float rotateDuration = 8f;   // 8秒間回転
    public float rotateSpeed = 15f;     // 回転速度

    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= startDelay &&
            timer <= startDelay + rotateDuration)
        {
            transform.Rotate(
                0,
                rotateSpeed * Time.deltaTime,
                0
            );
        }
    }
}
