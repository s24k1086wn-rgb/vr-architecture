using System.Collections;
using UnityEngine;

public class WoodenBoxFallEvent : MonoBehaviour
{
    [Header("木箱のRigidbody")]
    [SerializeField] private Rigidbody boxRigidbody;

    [Header("教習開始から動き始めるまでの秒数")]
    [SerializeField] private float startTime = 103f;

    [Header("木箱が移動する距離")]
    [SerializeField] private Vector3 moveDistance =
        new Vector3(1f, 0f, 0f);

    [Header("木箱が移動する時間")]
    [SerializeField] private float moveDuration = 0.5f;

    private Vector3 startPosition;
    private Coroutine eventCoroutine;
    private bool hasStarted;

    private void Awake()
    {
        if (boxRigidbody == null)
        {
            boxRigidbody = GetComponent<Rigidbody>();
        }

        if (boxRigidbody == null)
        {
            Debug.LogError(
                $"{gameObject.name}にRigidbodyがありません。"
            );

            enabled = false;
            return;
        }

        startPosition = boxRigidbody.position;

        // 開始前は木箱を固定する
        boxRigidbody.useGravity = false;
        boxRigidbody.isKinematic = true;

        // 最初に残っている速度を消す
        boxRigidbody.linearVelocity = Vector3.zero;
        boxRigidbody.angularVelocity = Vector3.zero;
    }

    public void StartWoodenBoxFallEvent()
    {
        if (hasStarted || eventCoroutine != null)
        {
            return;
        }

        Debug.Log(
            $"木箱イベント待機開始：{gameObject.name} " +
            $"{startTime}秒後に動きます。"
        );

        eventCoroutine = StartCoroutine(EventSequence());
    }

    private IEnumerator EventSequence()
    {
        // STARTボタンを押してから指定時間待つ
        yield return new WaitForSeconds(startTime);

        hasStarted = true;

        Vector3 endPosition = startPosition + moveDistance;
        float elapsedTime = 0f;

        Debug.LogWarning(
            $"木箱の移動開始：{gameObject.name}"
        );

        // 固定状態のまま木箱を横へ動かす
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / moveDuration);

            Vector3 nextPosition = Vector3.Lerp(
                startPosition,
                endPosition,
                progress
            );

            boxRigidbody.MovePosition(nextPosition);

            yield return new WaitForFixedUpdate();
        }

        boxRigidbody.position = endPosition;

        // 横へ移動したあと、重力で落下させる
        boxRigidbody.isKinematic = false;
        boxRigidbody.useGravity = true;

        eventCoroutine = null;

        Debug.LogWarning(
            $"木箱の落下開始：{gameObject.name}"
        );
    }
}
