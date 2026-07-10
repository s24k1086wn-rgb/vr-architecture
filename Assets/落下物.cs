using UnityEngine;

public class FallAfterTime : MonoBehaviour
{
    public float fallStartTime = 103f; // 1分43秒後
    private Rigidbody rb;
    private bool hasStartedFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!hasStartedFalling && Time.time >= fallStartTime)
        {
            hasStartedFalling = true;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }
}
