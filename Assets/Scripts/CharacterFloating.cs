using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CharacterFloating : MonoBehaviour
{
    float startPos;

    void Start()
    {
        startPos = transform.position.y;
        StartCoroutine(CharacterFloatIE());
    }

    IEnumerator CharacterFloatIE()
    {
        while (true)
        {
            transform.DOMoveY(startPos + 0.25f, 2.5f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(2.5f);
            transform.DOMoveY(startPos, 2.5f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(2.5f);
            yield return new WaitForSeconds(0.25f);
        }
    }
}
