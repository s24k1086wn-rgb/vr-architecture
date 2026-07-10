using UnityEngine;

public class RollerBackMove : MonoBehaviour
{
    public float startTime = 16f;      // 教習開始から16秒後
    public float moveDistance = 5f;    // バックする距離(m)
    public float moveSpeed = 1f;       // バックする速さ(m/s)

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        startPosition = transform.position;

        // ローカル座標で後ろ方向
        targetPosition = startPosition - transform.forward * moveDistance;
    }

    void Update()
    {
        if (!isMoving && Time.time >= startTime)
        {
            isMoving = true;
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
            }
        }
    }
}
