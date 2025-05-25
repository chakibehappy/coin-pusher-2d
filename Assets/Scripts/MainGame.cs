using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using Spine.Unity;
using TMPro;
using UnityEngine.Networking;

public class MainGame : MonoBehaviour
{
    [SerializeField] private APIManager API;
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private AudioManager Audio;
    public List<GameObject> coinPrefab;


    public Transform minX, maxX, minZ, maxZ;
    public float delayPerCoin = 0.05f;

    public List<GameObject> coinsOnPlatform = new();

    [SerializeField] private CoinSpawner coinSpawner;

    public List<GameObject> frontRowCoins = new();


    [SerializeField] private Transform spinWheelUI;
    [SerializeField] private Transform spinWheelObj;
    [SerializeField] private SpinWheel spinWheel;

    public GameObject redGemPrefab;
    public GameObject goldBarPrefab;

    public Transform coinFallingPoint;
    public Transform frontColliderPoint;

    public Transform orbPosition;
    public Transform coinTargetPos;

    [SerializeField] private SkeletonAnimation charSpine;
    [SerializeField]
    private string[] charAnimNames = new string[] {
        "idle", "launch idle", "launch start", "spinwheel idle", "spinwheel start"
    };

    [SerializeField] private SkeletonAnimation orbSpine;
    [SerializeField] 
    private string[] orbAnimNames = new string[] {
        "idle", "launch idle", "launch start", "idle disable"
    };

    [SerializeField] private SkeletonGraphic spinwheelSpine;
    [SerializeField]
    private string[] spinwheelAnimNames = new string[] {
        "Start", "Loop", "End"
    };

    [SerializeField] private float minForwardForce = 2.5f;
    [SerializeField] private float maxForwardForce = 3.5f;

    [SerializeField] private float minUpwardForce = 3.5f;
    [SerializeField] private float maxUpwardForce = 5f;
    [SerializeField] float sidewaysForceRange = 1f;

    public CoinPusherPhysics pusher;
    public CoinDestroyer coinDestroyer;

    [SerializeField] private SkeletonGraphic rewardSpine;
    [SerializeField]
    private string[] rewardAnimNames = new string[] {
        "Spawn", "Idle"
    };
    [SerializeField] private TextMeshProUGUI txtWinningNumber;

    [SerializeField] private SkeletonAnimation blackHoleSpine;
    [SerializeField] private string[] blackHoleAnimName = new string[] { "start", "idle", "end" };

    public bool isActiveSession = false;

    BetResponse currentBetResponse;

    public int minTresholdCoinCount = 100;

    public GameObject bonusText, freeCoinText;
    public float delayShowingText = 0.5f;


    private void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Coin"), LayerMask.NameToLayer("Coin"), true);

        spinWheelUI.gameObject.SetActive(false);
        PlayCharacterAndOrbAnimationIdle();
#if UNITY_EDITOR
        StartCoroutine(GetLoginDataIE());
#else
        StartCoroutine(API.GetAPIFromConfig(GetUserData));
#endif
    }

    void GetUserData()
    {
        StartCoroutine(GetUserDataIE());
    }

    IEnumerator GetUserDataIE()
    {
        UnityWebRequest request = UnityWebRequest.Get(API.GetDataUserAPI());
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            API.Log("Error: " + request.error);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            API.Log(responseJson);
            UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
            PassingUserData(response);
        }
    }

    IEnumerator GetLoginDataIE()
    {
        yield return StartCoroutine(API.TriggerLoginIE(PassingUserData));
    }


    void PassingUserData(UserDataResponse response)
    {
        if (response != null)
        {
            menuManager.SetUserData(response);
            menuManager.ShowButtonPlayAndTutorial();
        }
        else
        {
            StartCoroutine(GetUserDataIE());
        }
    }


    void PlayCharacterAndOrbAnimationIdle(bool enableOrb = true)
    {
        if (SpineHelper.GetCurrentAnimationName(charSpine) != charAnimNames[0])
            SpineHelper.PlayAnimation(charSpine, charAnimNames[0], true);
        SpineHelper.PlayAnimation(orbSpine, orbAnimNames[enableOrb? 0 : 3], true);
    }

    IEnumerator PlayCharacterAndOrbAnimationLaunchIE()
    {
        SpineHelper.PlayAnimation(charSpine, charAnimNames[2], false);
        SpineHelper.PlayAnimation(orbSpine, orbAnimNames[2], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(charSpine, charAnimNames[2]));
        SpineHelper.PlayAnimation(charSpine, charAnimNames[1], true);
        SpineHelper.PlayAnimation(orbSpine, orbAnimNames[1], true);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(charSpine, charAnimNames[1]));
        PlayCharacterAndOrbAnimationIdle(false);
    }

    IEnumerator ShowSpinWheelIE(string drop_result)
    {
        SpineHelper.PlayAnimation(charSpine, charAnimNames[4], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(charSpine, charAnimNames[4]));
        spinWheelObj.localScale = Vector3.zero;
        spinWheelUI.gameObject.SetActive(true);

        Audio.PlaySFX(3);
        SpineHelper.PlayAnimation(spinwheelSpine, spinwheelAnimNames[0], false);
        spinWheelUI.GetComponent<Image>().DOFade(200f/255f, 0.5f);
        spinWheelObj.DOScale(Vector3.one, 0.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);

        int result = spinWheel.GetSpinwheelItemIndex(drop_result);

        SpineHelper.PlayAnimation(spinwheelSpine, spinwheelAnimNames[1], true);
        yield return StartCoroutine(spinWheel.StartSpinIE(result));

        SpineHelper.PlayAnimation(spinwheelSpine, spinwheelAnimNames[2], false);
        spinWheelObj.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear);
        spinWheelUI.GetComponent<Image>().DOFade(0, 0.5f);
        yield return new WaitForSeconds(0.5f);

        PlayCharacterAndOrbAnimationIdle(false);
        spinWheelUI.gameObject.SetActive(false);

        if (drop_result == "diamond")
        {
            Vector3 pos = new Vector3(
                (coinSpawner.minX.position.x + coinSpawner.maxX.position.x) / 2f,
                maxX.position.y,
                (coinSpawner.minZ.position.z + coinSpawner.maxZ.position.z) / 2f
            );
            GameObject redGem = Instantiate(redGemPrefab, pos, Quaternion.Euler(Random.Range(0, 360), 0, 0));
            BonusDrop bonus = redGem.GetComponent<BonusDrop>();
            bonus.coinFallingPoint = coinFallingPoint.position;
            bonus.multiplier = spinWheel.segmentLabels[result].multiplier;
            while (redGem != null)
            {
                yield return null;
            }
        }
        else if (drop_result == "blackhole")
        {
            blackHoleSpine.gameObject.SetActive(true);
            SpineHelper.PlayAnimation(blackHoleSpine, blackHoleAnimName[0], false);
            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(blackHoleSpine, blackHoleAnimName[0]));
            SpineHelper.PlayAnimation(blackHoleSpine, blackHoleAnimName[1], true);

            coinsOnPlatform = coinsOnPlatform.Where(coin => coin != null)
                .OrderByDescending(coin => coin.transform.position.y)
                .ToList();

            List<GameObject> coinsToRemove = new();
            int targetCount = 10;
            int coinCount = 0;

            for (int i = 0; i < coinsOnPlatform.Count; i++)
            {
                Coin coin = coinsOnPlatform[i].GetComponent<Coin>();
                if (coin.isSpawnedByPlayer)
                {
                    CoinsGoToBlackHole(coinsOnPlatform[i], coinsToRemove);
                }
                else
                {
                    if (coinCount < targetCount)
                    {
                        coinCount++;
                        CoinsGoToBlackHole(coinsOnPlatform[i], coinsToRemove);
                    }
                }
            }

            yield return new WaitForSeconds(1.5f);
            foreach (var obj in coinsToRemove)
            {
                coinsOnPlatform.Remove(obj);
                Destroy(obj.gameObject);
            }

            SpineHelper.PlayAnimation(blackHoleSpine, blackHoleAnimName[2], false);
            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(blackHoleSpine, blackHoleAnimName[2]));
            blackHoleSpine.gameObject.SetActive(false);
        }
        else if (drop_result == "gold_bar")
        {
            Vector3 pos = new Vector3(
                (coinSpawner.minX.position.x + coinSpawner.maxX.position.x) / 2f,
                maxX.position.y,
                (coinSpawner.minZ.position.z + coinSpawner.maxZ.position.z) / 2f
            );
            GameObject goldBar = Instantiate(goldBarPrefab, pos, Quaternion.Euler(Random.Range(0, 360), 0, 0));
            BonusDrop bonus = goldBar.GetComponent<BonusDrop>();
            bonus.coinFallingPoint = coinFallingPoint.position;
            bonus.multiplier = spinWheel.segmentLabels[result].multiplier;
            while (goldBar != null)
            {
                yield return null;
            }
        }
        else if (drop_result == "extra_drop")
        {
            yield return StartCoroutine(LaunchingCoinsIE(10));
        }
        PlayCharacterAndOrbAnimationIdle(false);
    }

    void CoinsGoToBlackHole(GameObject coinObj, List<GameObject> coinsToRemove)
    {
        coinsToRemove.Add(coinObj);
        coinObj.GetComponent<Rigidbody>().isKinematic = true;
        coinObj.transform.DOMove(orbPosition.position, 1.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            coinObj.SetActive(false);
        });
    }


    IEnumerator LaunchingCoinsIE(int coinCount, int bonusCoinCount = 0)
    {
        StartCoroutine(PlayCharacterAndOrbAnimationLaunchIE());
        Audio.PlaySFX(coinCount == 1 ? 0 : 1);

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.1f, 0.1f),
                0,
                Random.Range(-0.1f, 0.1f)
            );
            
            GameObject coinObj = Instantiate(coinPrefab[Random.Range(0, 3)], orbPosition.position + randomOffset, Quaternion.identity);
            coinObj.layer = LayerMask.NameToLayer("Coin");
            Rigidbody rb = coinObj.GetComponent<Rigidbody>();
            yield return null;
            coinObj.GetComponent<Coin>().isSpawnedByPlayer = true;
            Vector3 forceDirection =
                -orbPosition.forward * Random.Range(minForwardForce, maxForwardForce) +
                orbPosition.up * Random.Range(minUpwardForce, maxUpwardForce) +
                orbPosition.right * Random.Range(-sidewaysForceRange, sidewaysForceRange);
            
            rb.AddForce(forceDirection, ForceMode.Impulse);
            Vector3 randomTorque = new Vector3(Random.Range(-10f, 10f), 0, 0);
            rb.AddTorque(randomTorque, ForceMode.Impulse);
            coinsOnPlatform.Add(coinObj);
            yield return new WaitForSeconds(delayPerCoin);
        }
        yield return new WaitForFixedUpdate();
        StartCoroutine(ResetPhysicLayerIE());
    }

    IEnumerator ResetPhysicLayerIE()
    {
        yield return new WaitForSeconds(1.25f);
        foreach (GameObject coin in coinsOnPlatform)
        {
            if (coin != null)
                coin.layer = LayerMask.NameToLayer("Default");
        }
    }

    void ParseBetResponseData(BetResponse betResponse)
    {
        currentBetResponse = betResponse;
    }

    public IEnumerator InsertCoinIE(int coinCount)
    {
        yield return StartCoroutine(API.SendBetRequest(coinCount, Mathf.RoundToInt(menuManager.GetCurrentBet() * coinCount), ParseBetResponseData));

        if(currentBetResponse.status == true)
        {
            yield return StartCoroutine(LaunchingCoinsIE(coinCount));

            coinsOnPlatform = coinsOnPlatform.Where(coin => coin != null)
                .OrderBy(coin => coin.transform.position.z)
                .ThenByDescending(coin => coin.transform.position.y).ToList();
            float frontZ = coinsOnPlatform.Min(coin => coin.transform.position.z);

            float margin = 0.5f;
            frontRowCoins = coinsOnPlatform
                .Where(coin => coin != null && Mathf.Abs(coin.transform.position.z - frontZ) < margin)
                .OrderBy(coin => coin.transform.position.z)
                .ThenByDescending(coin => coin.transform.position.y)
                .ToList();

            //List<GameObject> bonusCoins = coinsOnPlatform
            //    .Where(coin => coin != null && Mathf.Abs(coin.transform.position.z - frontZ) < margin)
            //    .OrderBy(coin => coin.transform.position.z)
            //    .ThenByDescending(coin => coin.transform.position.y)
            //    .ToList();

            //List<GameObject> platformBonusCoins = coinsOnPlatform.FindAll(c => c != null && c.GetComponent<Coin>().Value.name == "Bonus");
            //List<GameObject> frontBonusCoins = frontRowCoins.FindAll(c => c.GetComponent<Coin>().Value.name == "Bonus");
            //List<GameObject> restOfBonusCoins = platformBonusCoins.Where(c => !frontBonusCoins.Contains(c)).ToList();


            List<GameObject> coinsToRemove = new();

            float multiplier = currentBetResponse.data.game_result.result.total_multiplier;

            List<GameObject> coinResult = SplitMultiplier(multiplier);


            int coinFallCount = coinResult.Count;
            API.Log($"Target Coin to Drop : {coinFallCount}");

            yield return new WaitForSeconds(1f);
            // waiting for all coins let say down on platform and wait till pusher on front power!
            while (!pusher.isPushing)
            {
                yield return null;
            }

            if (currentBetResponse.data.total_win > 0)
            {
                if (coinFallCount > 0)
                {
                    Audio.PlaySFX(2);
                    coinDestroyer.isCheckingCoinFall = true;
                    for (int i = 0; i < coinFallCount; i++)
                    {
                        Coin coin = coinResult[i].GetComponent<Coin>();
                        coinsToRemove.Add(coinResult[i]);
                        coin.DropingDown(coinFallingPoint.position, 0.75f);
                        yield return null;
                    }

                    foreach (var coin in coinsToRemove)
                    {
                        frontRowCoins.Remove(coin);
                        coinsOnPlatform.Remove(coin);
                    }
                    yield return null;

                    for (int i = 0; i < coinsOnPlatform.Count; i++)
                    {
                        if (coinsOnPlatform[i] != null)
                        {
                            Coin coin = coinsOnPlatform[i].GetComponent<Coin>();
                            bool isFrontCoin = coinSpawner.IsOnFrontAreaCollider(coinsOnPlatform[i]);
                            if (!isFrontCoin)
                            {
                                if (coin.stackLevel <= 2 && !coin.isSpawnedByPlayer)
                                {
                                    coin.MoveForward(frontColliderPoint.position);
                                }
                            }
                            else
                            {
                                coin.MoveToFrontPosition(frontColliderPoint.position);
                            }
                            coinsOnPlatform[i].GetComponent<Rigidbody>().isKinematic = isFrontCoin;
                        }
                    }
                }

                // wait until all coins is done falling!
                while (coinDestroyer.isCheckingCoinFall)
                {
                    yield return null;
                }


                if(currentBetResponse.data.game_result.result.itemDrops.Count > 0)
                {
                    bonusText.SetActive(true);
                    bonusText.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), delayShowingText, 1).SetEase(Ease.Linear);
                    yield return new WaitForSeconds(delayShowingText);
                    bonusText.SetActive(false);
                    string drop_result = currentBetResponse.data.game_result.result.itemDrops[0];
                    spinWheel.FillSpinwheel(currentBetResponse.data.game_result.result.spinWheelData.items);
                    yield return StartCoroutine(ShowSpinWheelIE(drop_result));
                }

                if (coinsOnPlatform.Count <= minTresholdCoinCount)
                {
                    freeCoinText.SetActive(true);
                    freeCoinText.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), delayShowingText, 1).SetEase(Ease.Linear);
                    yield return new WaitForSeconds(delayShowingText);
                    freeCoinText.SetActive(false);
                    int coinToBeSpawned = coinSpawner.maxCoins - coinsOnPlatform.Count;
                    coinSpawner.SpawnCoins(coinToBeSpawned);
                }
            }

            // Recalculate stack levels
            coinSpawner.RecalculateCoinStacks(coinsOnPlatform);

            float totalWin = currentBetResponse.data.total_win;
            if (totalWin > 0)
            {
                Audio.PlaySFX(6);
                rewardSpine.gameObject.SetActive(true);
                txtWinningNumber.gameObject.SetActive(true);
                StartCoroutine(PlayRewardSpineAnimationIE());
                float delayText = 0.5f;
                txtWinningNumber.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), delayText, 1).SetEase(Ease.Linear);
                yield return StartCoroutine(AnimateNumberIE(0, totalWin, delayText));
                menuManager.ChangePlayerBallance(currentBetResponse.data.balance.Split('.')[0]);
                yield return new WaitForSeconds(0.25f);
                txtWinningNumber.gameObject.SetActive(false);
                rewardSpine.gameObject.SetActive(false);
            }
            yield return null;
            PlayCharacterAndOrbAnimationIdle();
            EndCoinLaunchSession();

            if (menuManager.isAutoPlay)
            {
                menuManager.InitiateAutoPlay();
            }
            else
            {
                menuManager.ShowLaunchButtonHint(menuManager.idleTimeToShowHint);
            }
        }
        else
        {
            menuManager.ShowMessageInfo(currentBetResponse.message);
            EndCoinLaunchSession();
            if (menuManager.isAutoPlay)
            {
                menuManager.SetAutoPlay();
            }
        }
    }

    void EndCoinLaunchSession()
    {
        isActiveSession = false;
        menuManager.EnableMenuButtons();
    }

    IEnumerator PlayRewardSpineAnimationIE()
    {
        SpineHelper.PlayAnimation(rewardSpine, rewardAnimNames[0], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(rewardSpine, rewardAnimNames[0]));
        SpineHelper.PlayAnimation(rewardSpine, rewardAnimNames[1], true);
    }

    IEnumerator AnimateNumberIE(float startValue, float endValue, float duration)
    {
        DOTween.To(() => startValue, x => startValue = x, endValue, duration).OnUpdate(() =>
        {
            txtWinningNumber.text = menuManager.playerCurrency + " " + StringHelper.MoneyFormat(startValue, menuManager.playerCurrency);
            //txtWinningNumber.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = menuManager.playerCurrency + " " + StringHelper.MoneyFormat(startValue);
            //txtWinningNumber.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = menuManager.playerCurrency + " " + StringHelper.MoneyFormat(startValue);
        });
        yield return new WaitForSeconds(duration);
    }

    private List<GameObject> SplitMultiplier(float totalMultiplier)
    {
        string val = totalMultiplier.ToString("F2"); 
        float cleaned = float.Parse(val);
        // Debug.Log(cleaned);

        List<GameObject> results = new();

        List<GameObject> frontGoldCoins = frontRowCoins.FindAll(c => c != null && c.GetComponent<Coin>().Value.name == "Gold");
        List<GameObject> platformGoldCoins = coinsOnPlatform.FindAll(c => c != null && c.GetComponent<Coin>().Value.name == "Gold");
        List<GameObject> restOfGoldCoins = platformGoldCoins.Where(c => !frontGoldCoins.Contains(c)).ToList();

        while (cleaned >= 1f)
        {
            if(frontGoldCoins.Count > 0)
            {
                frontGoldCoins[0].GetComponent<Coin>().multiplier = 1f;
                results.Add(frontGoldCoins[0]);
                frontGoldCoins.RemoveAt(0);
                cleaned -= 1f;
            }
            else
            {
                if(restOfGoldCoins.Count > 0)
                {
                    restOfGoldCoins[0].GetComponent<Coin>().multiplier = 1f;
                    results.Add(restOfGoldCoins[0]);
                    restOfGoldCoins.RemoveAt(0);
                    cleaned -= 1f;
                }
                else
                {
                    Debug.Log("No Gold Coins Left, continue to Silver Coins");
                    break;
                }
            }
        }


        List<GameObject> frontSilverCoins = frontRowCoins.FindAll(c => c != null && c.GetComponent<Coin>().Value.name == "Silver");
        List<GameObject> platformSilverCoins = coinsOnPlatform.FindAll(c => c != null && c.GetComponent<Coin>().Value.name == "Silver");
        List<GameObject> restOfSilverCoins = platformSilverCoins.Where(c => !frontSilverCoins.Contains(c)).ToList();

        while (cleaned >= 0.5f)
        {
            if (frontSilverCoins.Count > 0)
            {
                frontSilverCoins[0].GetComponent<Coin>().multiplier = 0.5f;
                results.Add(frontSilverCoins[0]);
                frontSilverCoins.RemoveAt(0);
                cleaned -= 0.5f;
            }
            else
            {
                if (restOfSilverCoins.Count > 0)
                {
                    restOfSilverCoins[0].GetComponent<Coin>().multiplier = 0.5f;
                    results.Add(restOfSilverCoins[0]);
                    restOfSilverCoins.RemoveAt(0);
                    cleaned -= 0.5f;
                }
                else
                {
                    Debug.Log("No Silver Coins Left, continue to Copper Coins");
                    break;
                }
            }
        }

        List<GameObject> frontCopperCoins = frontRowCoins.FindAll(c => c != null && c.GetComponent<Coin>().Value.name == "Copper");
        List<GameObject> platformCopperCoins = coinsOnPlatform.FindAll(c => c != null && c.GetComponent<Coin>().Value.name == "Copper");
        List<GameObject> restOfCopperCoins = platformCopperCoins.Where(c => !frontCopperCoins.Contains(c)).ToList();


        while (cleaned >= 0.1f)
        {
            if (frontCopperCoins.Count > 0)
            {
                frontCopperCoins[0].GetComponent<Coin>().multiplier = 0.1f;
                results.Add(frontCopperCoins[0]);
                frontCopperCoins.RemoveAt(0);
                cleaned -= 0.1f;
            }
            else
            {
                if (restOfCopperCoins.Count > 0)
                {
                    restOfCopperCoins[0].GetComponent<Coin>().multiplier = 0.1f;
                    results.Add(restOfCopperCoins[0]);
                    restOfCopperCoins.RemoveAt(0);
                    cleaned -= 0.1f;
                }
                else
                {
                    Debug.Log("No Copper Coins Left, continue to instantiate some based from what we less");
                    break;
                }
            }
        }

        // add the rest to the last of result coin
        if (cleaned > 0)
        {
            results[results.Count - 1].GetComponent<Coin>().multiplier += cleaned;
        }

        return results;
    }

}
