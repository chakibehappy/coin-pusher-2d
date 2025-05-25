using UnityEngine;

public class TipCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            other.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}
