using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CoinDestroyer : MonoBehaviour
{
    public GameObject multiplierTextPrefab;
    public Transform startPosY, midEndPosY, endPosY;
    public Transform minPosX, maxPosX;
    public Transform canvas;

    public int totalCoinFall = 0;
    public bool isCheckingCoinFall = false;
    bool startCheck = false;
    public float fallSessionDelay = 2f;

    public Camera UICamera;
    public Canvas uiCanvas;

    private void OnTriggerEnter(Collider other)
    {
        Vector3 screenPos = UICamera.WorldToScreenPoint(other.transform.position);
        Vector2 anchoredPos;
        RectTransform canvasRect = canvas as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, UICamera, out anchoredPos);

        // Mirror correction if the camera is facing backward
        anchoredPos.x = -anchoredPos.x; // Flip X-axis
        anchoredPos.x = Mathf.Clamp(anchoredPos.x, minPosX.localPosition.x, maxPosX.localPosition.x);

        // Set the spawn position
        Vector3 spawnPos = new Vector3(anchoredPos.x, startPosY.localPosition.y, 0);


        if (other.CompareTag("Coin"))
        {

            Coin coin = other.GetComponent<Coin>();
            if (coin.isDropped)
            {
                //Debug.Log("Intended to Fall");
                ShowMultiplierText(spawnPos, coin.multiplier, Color.white);
            }
            else
            {
                //Debug.Log("Falling naturally");
            }
            Destroy(other.gameObject);
            
            totalCoinFall++;
            if (isCheckingCoinFall && !startCheck)
            {
                StartCoroutine(CheckCoinFallIE());
            }

        }

        if (other.CompareTag("Gem") || other.CompareTag("Gold"))
        {
            BonusDrop bonus = other.GetComponent<BonusDrop>();
            ShowMultiplierText(spawnPos, bonus.multiplier, Color.green, other.gameObject);
        }
    }

    IEnumerator CheckCoinFallIE()
    {
        isCheckingCoinFall = true;
        startCheck = true;
        yield return new WaitForSeconds(fallSessionDelay);
        // Debug.Log("Coin Fall : " + totalCoinFall);
        totalCoinFall = 0;
        isCheckingCoinFall = startCheck = false;
    }

    void ShowMultiplierText(Vector3 spawnPos, float multiplier, Color color, GameObject obj = null)
    {
        GameObject textObj = Instantiate(multiplierTextPrefab, canvas);
        textObj.transform.localPosition = spawnPos;
        string multiplierText;
        if(multiplier < 0.1f)
        {
            multiplierText = multiplier.ToString("F2");
        }
        else
        {
            multiplierText = multiplier < 1 ? multiplier.ToString("F1") : multiplier.ToString();
        }
        textObj.GetComponent<TextMeshProUGUI>().text = multiplierText + "x";
        textObj.GetComponent<TextMeshProUGUI>().color = color;

        textObj.transform.DOMoveY(Random.Range(midEndPosY.position.y, endPosY.position.y), 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            textObj.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 1, 1).SetEase(Ease.Linear);
            textObj.GetComponent<TextMeshProUGUI>().DOFade(0, 1f).SetEase(Ease.Linear).OnComplete(() =>
            //textObj.transform.DOPunchScale(Vector3.one, 1, 1).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(textObj);
                if(obj != null)
                {
                    Destroy(obj);
                }
            });
        });
    }
}


