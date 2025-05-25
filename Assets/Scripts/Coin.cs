using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    Rigidbody coinRb;
    public bool isSpawnedByPlayer = false;
    public int stackLevel = 0;
    public bool isDropped = false;
    public float diameter;
    public float multiplier;
    public MeshRenderer meshRenderer;
    public CoinValue Value; 

    IEnumerator moveCoroutine;

    private void Start()
    {
        coinRb = GetComponent<Rigidbody>();
        diameter = GetComponent<Collider>().bounds.size.x;
        meshRenderer.sortingOrder = 500;
    }

    public void DropingDown(Vector3 coinFallingPoint, float force = 1f)
    {
        isDropped = true;
        coinRb.isKinematic = false;
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        moveCoroutine = FallToPoint(coinFallingPoint, force);
        StartCoroutine(moveCoroutine);
    }

    public void MoveForward(Vector3 maxTargetPos)
    {
        Vector3 direction = Vector3.back;
        Vector3 desiredTarget = coinRb.position + direction * (diameter/4);

        // Clamp target so it never passes maxTargetPos.z
        if (desiredTarget.z < maxTargetPos.z)
        {
            desiredTarget.z = maxTargetPos.z;
        }

        MoveToPosition(desiredTarget, 0.5f);
    }

    public void MoveToFrontPosition(Vector3 maxTargetPos)
    {
        Vector3 desiredTarget = new(coinRb.position.x, coinRb.position.y, maxTargetPos.z);
        MoveToPosition(desiredTarget, 2.5f);
    }


    void MoveToPosition(Vector3 movePoint, float force = 10f)
    {
        coinRb.isKinematic = false;
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        moveCoroutine = MoveToPoint(movePoint, force);
        StartCoroutine(moveCoroutine);
    }


    private IEnumerator MoveToPoint(Vector3 targetPoint, float force)
    {
        float threshold = 0.05f;
        while (Vector3.Distance(coinRb.position, targetPoint) > threshold)
        {
            coinRb.MovePosition(Vector3.MoveTowards(coinRb.position, targetPoint, Time.fixedDeltaTime * force));
            yield return null;
        }
        coinRb.MovePosition(targetPoint);
    }

    private IEnumerator FallToPoint(Vector3 targetPoint, float force)
    {
        float threshold = 0.05f;

        while (Vector3.Distance(coinRb.position, new Vector3(coinRb.position.x, coinRb.position.y, targetPoint.z)) > threshold)
        {
            coinRb.MovePosition(Vector3.MoveTowards(
                coinRb.position,
                new Vector3(coinRb.position.x, coinRb.position.y, targetPoint.z),
                Time.deltaTime * force
            ));

            yield return null;
        }

        coinRb.MovePosition(new Vector3(coinRb.position.x, coinRb.position.y, targetPoint.z));
    }

    public void MoveForwardByForce(float force = 2f)
    {
        if (coinRb == null || Camera.main == null) return;

        Vector3 forceDirection = -Camera.main.transform.forward * force;
        coinRb.AddForce(forceDirection, ForceMode.Impulse);
    }
}

[System.Serializable]
public class CoinValue
{
    public string name;
    public bool fixedMultiplier = true;
    public float multiplier;
    public float minMultiplier;
    public float maxMultiplier;
} 
