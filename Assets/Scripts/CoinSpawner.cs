using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CoinSpawner : MonoBehaviour
{
    public CoinSpawnDistribution[] coins; 

    public int maxCoins = 150; 
    public float stackHeight = 0.2f;
    //public PhysicsMaterial slipperyMaterial;
    //public PhysicsMaterial staticMaterial;
    public Transform minX, maxX, minZ, maxZ;

    private Dictionary<Vector2, int> coinStacks = new Dictionary<Vector2, int>();
    private float platformTopY;
    private float coinDiameter;

    public Transform platformParent;
    public float platformAngle = -5;


    public float stackOffsetRange = 0.01f;

    [SerializeField] private MainGame game;
    int[] coinDistributionCounts;

    List<GameObject> coinPools = new();

    void Start()
    {
        // Get platform's top Y position
        Collider platformCollider = GetComponent<Collider>();
        platformTopY = platformCollider ? platformCollider.bounds.max.y : transform.position.y;

        // Get coin diameter dynamically
        GameObject tempCoin = Instantiate(coins[0].coinPrefab);
        Collider coinCollider = tempCoin.GetComponent<Collider>();
        coinDiameter = coinCollider.bounds.size.x; 
        stackHeight = coinCollider.bounds.size.y;
        Destroy(tempCoin); // Cleanup

        SpawnCoins();
    }

    public void SetCoinDistributionCount(int coinTotal, bool overWriteDist = false, int[] newDist = null)
    {
        if (coinTotal == 0)
        {
            coinTotal = maxCoins;
        }

        coinDistributionCounts = new int[coins.Length];
        int total = 0;
        for (int i = 0; i < coins.Length - 1; i++) 
        {
            coinDistributionCounts[i] = Mathf.RoundToInt(coinTotal * coins[i].spawnPercentage * 0.01f);
            total += coinDistributionCounts[i];
        }
        coinDistributionCounts[coins.Length - 1] = coinTotal - total;
    }


    public void FillCoinPools()
    {
        coinPools.Clear();
        for (int i = 0; i < coinDistributionCounts.Length; i++)
        {
            for (int j = 0; j < coinDistributionCounts[i]; j++)
            {
                GameObject c = Instantiate(coins[i].coinPrefab);
                c.SetActive(false);
                coinPools.Add(c);
            }
            if(i == 2)
            {
                // shuffle copper and silver coins first before continue
                coinPools = coinPools.OrderBy(x => Random.value).ToList();
            }
        }
    }


    public void SpawnCoins(int coinTotal = 0)
    {
        SetCoinDistributionCount(coinTotal);
        FillCoinPools();

        platformParent.transform.rotation = Quaternion.identity;
        int coinCount = 0;

        if(coinTotal == 0)
        {
            coinTotal = maxCoins;
        }

        while (coinCount < coinTotal)
        {
            // Snap positions to grid based on coin size
            float randomX = Mathf.Round(Random.Range(minX.position.x, maxX.position.x) / coinDiameter) * coinDiameter;
            float randomZ = Mathf.Round(Random.Range(minZ.position.z, maxZ.position.z) / coinDiameter) * coinDiameter;
            Vector2 key = new Vector2(randomX, randomZ);

            // Get stack height for this position
            int stackLevel = coinStacks.ContainsKey(key) ? coinStacks[key] : 0;
            float yPosition = platformTopY + (stackLevel * stackHeight);

            // Apply random slight offset for a natural stack (only for 2nd coin and above)
            float offsetX = (stackLevel > 0) ? Random.Range(-stackOffsetRange, stackOffsetRange) : 0;
            float offsetZ = (stackLevel > 0) ? Random.Range(-stackOffsetRange, stackOffsetRange) : 0;

            //Spawn the coin
            //GameObject coin = Instantiate(coins[0].coinPrefab,
            //   new Vector3(randomX + offsetX, yPosition, randomZ + offsetZ),
            //   Quaternion.Euler(90, 0, 0));

            GameObject coin = coinPools[coinCount];
            coin.transform.position = new Vector3(randomX + offsetX, yPosition, randomZ + offsetZ);
            coin.transform.rotation = Quaternion.Euler(90, 0, 0);
            coin.SetActive(true);

            bool is2ndFrontCoin = (randomZ <= minZ.position.z + (coinDiameter * 2));
            if (is2ndFrontCoin)
            {
                //coin.GetComponent<Renderer>().material.color = Color.green;
            }
            
            // set 1st and 2nd front coins to be kinematic
            bool isFrontCoin = (randomZ <= minZ.position.z + (coinDiameter));
            if (isFrontCoin)
            {
                //coin.GetComponent<Renderer>().material.color = Color.red;
            }

            coin.GetComponent<Rigidbody>().isKinematic = isFrontCoin || is2ndFrontCoin;

            // Apply physics materials
            Collider coinCol = coin.GetComponent<Collider>();
            if (coinCol)
            {
                //coinCol.material = (stackLevel == 0) ? slipperyMaterial : staticMaterial;
                //coinCol.material = staticMaterial;
            }
            coin.transform.SetParent(platformParent);
            coin.GetComponent<Coin>().stackLevel = stackLevel;
            game.coinsOnPlatform.Add(coin);

            // Update stack level
            coinStacks[key] = stackLevel + 1;

            coinCount++;
        }

        // Apply platform angle
        platformParent.transform.rotation = Quaternion.Euler(platformAngle, 0, 0);
    }


    public bool IsOnFrontAreaCollider(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }
        return obj.transform.position.z <= minZ.position.z + (coinDiameter);
    }

    public bool IsOnBackArea(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }
        return obj.transform.position.z >= maxZ.position.z - (coinDiameter)/2;
    }

    public bool IsOnMiddleArea(GameObject obj)
    {
        if (obj == null) return false;
        float frontZ = minZ.position.z + coinDiameter;
        float backZ = maxZ.position.z - coinDiameter / 2f;

        float oneThird = (backZ - frontZ) / 3f;
        float middleStartZ = frontZ + oneThird;       
        float middleEndZ = backZ - oneThird;          
        float objZ = obj.transform.position.z;

        return objZ > middleStartZ && objZ < middleEndZ;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Coin coin = collision.gameObject.GetComponent<Coin>();
            if (coin.isSpawnedByPlayer)
            {
                coin.isSpawnedByPlayer = false;
            }
            //coin.stackLevel = 0;
        }
    }

    public void RecalculateCoinStacks(List<GameObject> coins)
    {
        coinStacks.Clear(); // Reset the old stack data

        foreach (var coin in coins.Where(c => c != null))
        {
            Vector3 pos = coin.transform.position;
            Vector2 key = new Vector2(
                Mathf.Round(pos.x / coinDiameter) * coinDiameter,
                Mathf.Round(pos.z / coinDiameter) * coinDiameter
            );

            int currentStack = coinStacks.ContainsKey(key) ? coinStacks[key] : 0;
            coinStacks[key] = currentStack + 1;

            // Assign stack level to coin
            coin.GetComponent<Coin>().stackLevel = currentStack;
        }
    }

}

[System.Serializable]
public class CoinSpawnDistribution
{
    public GameObject coinPrefab;
    public int spawnPercentage;
}