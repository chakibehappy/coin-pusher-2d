using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPusherPhysics : MonoBehaviour
{
    public float speed = 2f;
    public Transform startPos, endPos;
    Rigidbody rb;
    bool movingToEnd = true;
    public bool isPushing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(MovePusher());
    }

    IEnumerator MovePusher()
    {
        while (true)
        {
            isPushing = false;
            Vector3 targetPos = movingToEnd ? endPos.position : startPos.position;

            while (Vector3.Distance(rb.position, targetPos) > 0.01f)
            {
                rb.MovePosition(Vector3.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime));
                yield return new WaitForFixedUpdate(); // Ensures physics consistency
            }

            movingToEnd = !movingToEnd; // Reverse direction after reaching target
            if(!movingToEnd)
            {
                isPushing = true;
                yield return null;
            }
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Coin coin = collision.gameObject.GetComponent<Coin>();
            if (coin.isSpawnedByPlayer)
            {
                Rigidbody coinRb = collision.gameObject.GetComponent<Rigidbody>();
                if (coinRb != null)
                {
                    collision.transform.SetParent(transform);
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Rigidbody coinRb = collision.gameObject.GetComponent<Rigidbody>();
            if (coinRb != null)
            {
                collision.transform.SetParent(null);
            }
        }
    }
}
