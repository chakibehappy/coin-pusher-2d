using UnityEngine;

public class FrontCoinsCollider : MonoBehaviour
{
    [SerializeField] private MainGame game;
    [SerializeField] private CoinSpawner coinSpawner;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            //if (coinSpawner.IsOnFrontAreaCollider(other.gameObject))
            //{
            //    other.gameObject.GetComponent<Renderer>().material.color = Color.red;
            //    if (!game.frontRowCoins.Contains(other.gameObject))
            //    {
            //        game.frontRowCoins.Add(other.gameObject);
            //    }
            //}

            other.gameObject.GetComponent<Renderer>().material.color = Color.red;
            if (!game.frontRowCoins.Contains(other.gameObject))
            {
                game.frontRowCoins.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            if (game.frontRowCoins.Contains(other.gameObject))
            {
                game.frontRowCoins.Remove(other.gameObject);
                other.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}
