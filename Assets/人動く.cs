using UnityEngine;

public class WorkerMove : MonoBehaviour
{
    public Transform target;      // 移動先
    public float speed = 1.5f;    // 移動速度
    public float startTime = 80f; // 教習開始から80秒後

    private bool isMoving = false;

    void Update()
    {
        // 80秒経過したら移動開始
        if (!isMoving && Time.time >= startTime)
        {
            isMoving = true;
        }

        // 移動
        if (isMoving)
        {
            // 移動方向を向く
            transform.LookAt(target);

            // 目的地へ移動
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime);

            // 到着したら停止
            if (Vector3.Distance(transform.position, target.position) < 0.05f)
            {
                isMoving = false;
            }
        }
    }
}
