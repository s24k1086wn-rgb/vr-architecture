using System.Collections;
using UnityEngine;

public class BrickFallEvent : MonoBehaviour
{
    [Header("教習開始から動き始めるまでの秒数")]
    [SerializeField] private float startTime = 103f;

    [Header("レンガが横へ動く距離")]
    [SerializeField] private Vector3 moveDistance =
        new Vector3(1f, 0f, 0f);

    [Header("レンガが動く時間")]
    [SerializeField] private float moveDuration = 0.5f;

    private Vector3 startPosition;
    private bool hasStarted;
    private Coroutine eventCoroutine;

    private void Awake()
    {
        startPosition = transform.position;
    }

    public void StartBrickFallEvent()
    {
        if (hasStarted || eventCoroutine != null)
        {
            return;
        }

        eventCoroutine = StartCoroutine(EventSequence());
    }

    private IEnumerator EventSequence()
    {
        // 教習開始から指定時間待つ
        yield return new WaitForSeconds(startTime);

        hasStarted = true;

        Vector3 endPosition = startPosition + moveDistance;
        float elapsedTime = 0f;

        Debug.LogWarning(
            $"レンガ落下イベント開始：{gameObject.name}"
        );

        // レンガを横へ少し動かす
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / moveDuration);

            transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                progress
            );

            yield return null;
        }

        transform.position = endPosition;
        eventCoroutine = null;
    }
}
