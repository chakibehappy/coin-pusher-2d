using System.Collections;
using UnityEngine;

public class BonusDrop : MonoBehaviour
{
    public float forwardForce = 5f;
    public float upwardForce = 2f;
    public float multiplier = 25f;

    IEnumerator moveCoroutine;

    private Rigidbody rb;
    public Vector3 coinFallingPoint;

    float timeMultiplier = 0.75f;
    bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    // Check if velocity is low enough to trigger a launch
    //    if (rb.linearVelocity.magnitude < 1f) // tweak this threshold as needed
    //    {
    //        Debug.Log("Launching object!");
    //        Vector3 cameraBackward = -Camera.main.transform.forward;
    //        Vector3 forceDirection = cameraBackward * forwardForce + Vector3.up * upwardForce;
    //        rb.AddForce(forceDirection, ForceMode.Impulse);
    //    }
    //}

    void OnCollisionEnter(Collision collision)
    {
        if (!isMoving)
        {
            rb.isKinematic = false;
            Vector3 cameraBackward = -Camera.main.transform.forward;
            Vector3 forceDirection = cameraBackward * forwardForce + Vector3.up * upwardForce;
            rb.AddForce(forceDirection, ForceMode.Impulse);
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            moveCoroutine = FallToPoint(coinFallingPoint);
            StartCoroutine(moveCoroutine);
            isMoving = true;
        }
    }

    private IEnumerator FallToPoint(Vector3 targetPoint)
    {
        float threshold = 0.05f;

        while (Vector3.Distance(rb.position, new Vector3(rb.position.x, rb.position.y, targetPoint.z)) > threshold)
        {
            rb.MovePosition(Vector3.MoveTowards(
                rb.position,
                new Vector3(rb.position.x, rb.position.y, targetPoint.z),
                Time.deltaTime * timeMultiplier
            ));

            yield return null;
        }

        rb.MovePosition(new Vector3(rb.position.x, rb.position.y, targetPoint.z));
    }
}

