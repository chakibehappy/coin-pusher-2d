using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    public string labelKey;

    private TMP_Text textComponent;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {

            if (LanguageManager.Instance.IsLanguageReady())
            {
                UpdateText();
            }
            else
            {
                LanguageManager.Instance.OnLanguageReady += UpdateText;
            }

            LanguageManager.Instance.OnLanguageChanged += UpdateText;
        }
    }

    void UpdateText()
    {
        if (textComponent != null)
        {
            textComponent.text = LanguageManager.Instance.GetLabel(labelKey);
        }
    }

    void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }
}
