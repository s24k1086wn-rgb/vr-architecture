using System.Collections;
using UnityEngine;

public class PlayerFallDetector : MonoBehaviour
{
    [Header("プレイヤーのCharacter Controller")]
    [SerializeField] private CharacterController characterController;

    [Header("落下時に表示する警告画面")]
    [SerializeField] private GameObject fallWarningPanel;

    [Header("危険と判定する落下距離")]
    [SerializeField] private float dangerousFallHeight = 2.5f;

    [Header("危険と判定する最低落下速度")]
    [SerializeField] private float minimumFallSpeed = 3f;

    [Header("警告を表示する時間")]
    [SerializeField] private float warningDisplayTime = 3f;

    private bool wasGrounded;
    private bool isFalling;

    private float highestY;
    private float fastestDownwardSpeed;

    private Coroutine warningCoroutine;

    private void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (characterController == null)
        {
            Debug.LogError("Character Controllerが設定されていません。");
            enabled = false;
            return;
        }

        if (fallWarningPanel != null)
        {
            fallWarningPanel.SetActive(false);
        }

        wasGrounded = characterController.isGrounded;
        highestY = transform.position.y;
    }

    private void Update()
    {
        bool isGrounded = characterController.isGrounded;
        float currentY = transform.position.y;

        // 地面から離れた瞬間
        if (wasGrounded && !isGrounded)
        {
            isFalling = true;
            highestY = currentY;
            fastestDownwardSpeed = 0f;
        }

        // 空中にいる間
        if (isFalling && !isGrounded)
        {
            if (currentY > highestY)
            {
                highestY = currentY;
            }

            float downwardSpeed = -characterController.velocity.y;

            if (downwardSpeed > fastestDownwardSpeed)
            {
                fastestDownwardSpeed = downwardSpeed;
            }
        }

       // --- 修正する部分（Update関数の一番下にある着地判定の中） ---

        // 着地した瞬間
        if (!wasGrounded && isGrounded && isFalling)
        {
            float fallDistance = highestY - currentY;

            Debug.Log(
                $"落下距離：{fallDistance:F2}m " +
                $"最大落下速度：{fastestDownwardSpeed:F2}m/s"
            );

            if (fallDistance >= dangerousFallHeight &&
                fastestDownwardSpeed >= minimumFallSpeed)
            {
                ShowFallWarning();
                
                // ★【追加】大元の脳みそ（ResultManager）に落下したよ！と通知する
                ResultManager rm = FindAnyObjectByType<ResultManager>();
                if (rm != null) rm.RegisterFall();
            }

            isFalling = false;
        }

        wasGrounded = isGrounded;
    }

    private void ShowFallWarning()
    {
        if (fallWarningPanel == null)
        {
            Debug.LogWarning("Fall Warning Panelが設定されていません。");
            return;
        }

        // すでに警告表示中なら、前の3秒計測を止める
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine = StartCoroutine(ShowWarningForSeconds());

        Debug.LogWarning("危険行動：高所から落下しました。");
    }

    private IEnumerator ShowWarningForSeconds()
    {
        fallWarningPanel.SetActive(true);

        yield return new WaitForSeconds(warningDisplayTime);

        fallWarningPanel.SetActive(false);
        warningCoroutine = null;
    }
}
