using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    public string currentLanguage = "en";

    private Dictionary<string, LangData> allLanguages;
    private LangData currentLang;

    public event Action OnLanguageChanged;

    public event Action OnLanguageReady;
    private bool isLanguageReady = false;

    public bool IsLanguageReady()
    {
        return isLanguageReady;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadLanguageFile());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator LoadLanguageFile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        string url = Application.absoluteURL;
        if (url.EndsWith("index.html"))
        {
            url = url.Substring(0, url.Length - "index.html".Length);
        }
        url += "languages.json";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ParseJson(www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Failed to load language file: " + www.error);
        }
#else
        TextAsset jsonFile = Resources.Load<TextAsset>("Localization/languages");
        if (jsonFile == null)
        {
            Debug.LogError("languages.json not found in Resources/Localization/");
            yield break;
        }

        ParseJson(jsonFile.text);
        yield break;
#endif
    }

    void ParseJson(string jsonText)
    {
        try
        {
            allLanguages = JsonConvert.DeserializeObject<Dictionary<string, LangData>>(jsonText);
            SetLanguage(currentLanguage);
            isLanguageReady = true;
            OnLanguageReady?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse language JSON: {e.Message}");
        }
    }

    public void SetLanguage(string langCode)
    {
        currentLanguage = langCode;
        if (allLanguages != null && allLanguages.TryGetValue(langCode, out var langData))
        {
            currentLang = langData;
        }
        else
        {
            Debug.LogWarning($"Language {langCode} not found. Falling back to English.");
            currentLang = allLanguages != null && allLanguages.ContainsKey("en") ? allLanguages["en"] : null;
        }
        OnLanguageChanged?.Invoke();
    }

    public string GetLabel(string id, string default_text = "")
    {
        return currentLang?.labels != null && currentLang.labels.TryGetValue(id, out var text)
            ? text
            : default_text;
    }

    public string GetTutorialHtml(int pageIndex)
    {
        return (currentLang?.info_contents?.pages != null && pageIndex < currentLang.info_contents.pages.Count)
            ? currentLang.info_contents.pages[pageIndex].content
            : "";
    }

    public int GetTutorialPageCount()
    {
        return currentLang?.info_contents?.pages?.Count ?? 0;
    }
}


[Serializable]
public class LangData
{
    public Dictionary<string, string> labels;
    public InfoContents info_contents;
}

[Serializable]
public class InfoContents
{
    public List<Page> pages;
}

[Serializable]
public class Page
{
    public string content;
}


