using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SpinWheel : MonoBehaviour
{
    [SerializeField] private AudioManager Audio;
    public List<SpinwheelLabel> segmentLabels;
    public float spinDuration = 5f;
    public int minFullSpin = 5;
    public int maxFullSpin = 10;
    public int bounceBackSpinLimit = 6;
    public float overlapDegree = 2;
    public float bounceMultiplier = 0.05f;
    public float bounceDelayMultiplier = 0.05f;
    private RectTransform wheelTransform;

    public int desiredIndex = -1;   // -1 for random spin, otherwise set to specific segment index
    public bool spinClockwise;

    public Image[] spinItemImages;
    public TextMeshProUGUI[] spinItemMultiplierText;

    public List<SpinwheelItem> spinWheelItems;

    public List<SpinwheelItemOrder> spinwheelOrder;

    void Start()
    {
        wheelTransform = GetComponent<RectTransform>();
    }

    Sprite GetSpinwheelSprite(string itemName)
    {
        for (int i = 0; i < spinWheelItems.Count; i++)
        {
            if (itemName == spinWheelItems[i].name)
                return spinWheelItems[i].sprite;
        }
        return null;
    }

    public void FillSpinwheel(List<SpinItem> items)
    {
        SpinwheelItemOrder selectedOrder = spinwheelOrder[Random.Range(0, spinwheelOrder.Count)];
        segmentLabels = new List<SpinwheelLabel>();
        foreach (string name in selectedOrder.names)
        {
            SpinItem match = items.FirstOrDefault(i => i.name == name);
            if (match != null)
            {
                segmentLabels.Add(new SpinwheelLabel
                {
                    name = match.name,
                    multiplier = match.multiplier
                });
                items.Remove(match);
            }
            else
            {
                Debug.LogWarning($"Spin item '{name}' not found or already used up.");
            }
        }


        //segmentLabels = new List<SpinwheelLabel>();
        //for (int i = 0; i < items.Count; i++)
        //{
        //    segmentLabels.Add(new SpinwheelLabel() 
        //    { 
        //        name = items[i].name, 
        //        multiplier = items[i].multiplier 
        //    });
        //}

        //segmentLabels = segmentLabels.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < segmentLabels.Count; i++)
        {
            spinItemImages[i].sprite = GetSpinwheelSprite(segmentLabels[i].name);
            spinItemMultiplierText[i].text = segmentLabels[i].multiplier <= 0 ? "EXTRA" : 
                segmentLabels[i].multiplier.ToString() + "x";
        }

    }

    public int GetSpinwheelItemIndex(string drop_result)
    {
        List<int> spinIndexs = new();
        for(int i = 0; i < segmentLabels.Count; i++)
        {
            if(drop_result == segmentLabels[i].name)
                spinIndexs.Add(i);
        }
        spinIndexs = spinIndexs.OrderBy(x => UnityEngine.Random.value).ToList();
        int ranResult = spinIndexs[Random.Range(0, spinIndexs.Count)];
        return ranResult;
    }

    public IEnumerator StartSpinIE(int result = -1)
    {
        if(result >= 0)
        {
            desiredIndex = result;
        }

        int extraSpins = Random.Range(minFullSpin, maxFullSpin); 
        float targetRotation = GetFinalRotationForSegment(desiredIndex);

        float finalRotation = !spinClockwise
            ? (targetRotation + extraSpins * 360)
            : (targetRotation - extraSpins * 360);

        StartCoroutine(PlayResultAudioIE());
        float additionalSpinDuration = extraSpins > bounceBackSpinLimit ? bounceDelayMultiplier * extraSpins : 0;
        Audio.PlaySFX(4);
        wheelTransform.DORotate(new Vector3(0, 0, finalRotation), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                if (extraSpins > bounceBackSpinLimit)
                {
                    float bounceBack = (bounceMultiplier * extraSpins); 
                    wheelTransform.DORotate(new Vector3(0, 0, finalRotation - bounceBack), additionalSpinDuration)
                        .SetEase(Ease.InOutSine);
                }
            });

        yield return new WaitForSeconds(spinDuration + additionalSpinDuration);
    }

    IEnumerator PlayResultAudioIE()
    {
        yield return new WaitForSeconds(spinDuration - 1f);
        Audio.StopSFX();
        Audio.PlaySFX(5);
    }

    float GetFinalRotationForSegment(int index)
    {
        if (index < 0)
            return Random.Range(0, 360);

        int segments = segmentLabels.Count;
        float segmentSize = 360f / segments;
        return Random.Range((index * segmentSize) + overlapDegree, ((index+1) * segmentSize) - overlapDegree);
    }
}

[System.Serializable]
public class SpinwheelItem
{
    public string name;
    public Sprite sprite;
}

[System.Serializable]
public class SpinwheelLabel
{
    public string name;
    public float multiplier;
}

[System.Serializable]
public class SpinwheelItemOrder
{
    public List<string> names;
}