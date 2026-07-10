using UnityEngine;

public class ExcavatorMove : MonoBehaviour
{
    public float startDelay = 38f;
    public float moveDuration = 8f;
    public float moveSpeed = 2f;

    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= startDelay &&
            timer <= startDelay + moveDuration)
        {
            transform.Translate(
                Vector3.forward * moveSpeed * Time.deltaTime
            );
        }
    }
}

