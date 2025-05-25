using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vuplex.WebView;

public class TutorialDisplay : MonoBehaviour
{
    private LanguageManager languageManager;
    public CanvasWebViewPrefab webViewPrefab;
    [SerializeField] private Transform indicatorContainer;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Sprite activeIndicator;
    [SerializeField] private Sprite inactiveIndicator;
    [SerializeField] private GameObject prevButton, nextButton;
    
    private List<GameObject> indicators;
    private int currentPage = 0;


    [SerializeField] private RectTransform tutorialScreenUI;
    [SerializeField] private List<GameObject> closeButtons;

    void Start()
    {
        languageManager = LanguageManager.Instance;
        if (languageManager.IsLanguageReady())
        {
            InitTutorial();
        }
        else
        {
            LanguageManager.Instance.OnLanguageReady += InitTutorial;
        }
    }

    void InitTutorial()
    {
        SetupPage();
        AssignButtons();
        SetPage(currentPage);
    }

    public void OpenTutorial(bool isOpen = true)
    {
        StartCoroutine(OpenTutorialIE(isOpen));
    }

    IEnumerator OpenTutorialIE(bool isOpen = true)
    {
        tutorialScreenUI.gameObject.SetActive(isOpen);
        if (isOpen)
        {
            yield return new WaitForEndOfFrame();
            InitTutorial();
        }
    }

    void SetupPage()
    {
        indicators = new();
        for (int i = indicatorContainer.childCount - 1; i > 0; i--)
        {
            Destroy(indicatorContainer.GetChild(i).gameObject);
        }

        int totalPages = languageManager.GetTutorialPageCount();
        for (int i = 0; i < totalPages; i++)
        {
            GameObject obj = Instantiate(indicatorPrefab, indicatorContainer);
            obj.SetActive(true);
            indicators.Add(obj);
            int index = i;
            EventTrigger triggerPage = obj.AddComponent<EventTrigger>();
            EventTrigger.Entry entryPage = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            entryPage.callback.AddListener((data) => {
                SetPage(index);
            });
            triggerPage.triggers.Add(entryPage);
        }

    }

    void AssignButtons()
    {
        EventTrigger triggerPrev = prevButton.GetComponent<EventTrigger>();
        if (triggerPrev == null)
            triggerPrev = prevButton.AddComponent<EventTrigger>();
        else
            triggerPrev.triggers.Clear();

        EventTrigger.Entry entryPrev = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryPrev.callback.AddListener((data) => { PreviousPage();  });
        triggerPrev.triggers.Add(entryPrev);

        EventTrigger triggerNext = nextButton.GetComponent<EventTrigger>();
        if (triggerNext == null)
            triggerNext = nextButton.AddComponent<EventTrigger>();
        else
            triggerNext.triggers.Clear();

        EventTrigger.Entry entryNext = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryNext.callback.AddListener((data) => { NextPage(); });
        triggerNext.triggers.Add(entryNext);


        for (int i = 0; i < closeButtons.Count; i++)
        {
            int index = i;
            EventTrigger triggerClose = closeButtons[index].GetComponent<EventTrigger>();
            if (triggerClose == null)
                triggerClose = closeButtons[index].AddComponent<EventTrigger>();
            else
                triggerClose.triggers.Clear();

            EventTrigger.Entry entryClose = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            entryClose.callback.AddListener((data) => { OpenTutorial(false); });
            triggerClose.triggers.Add(entryClose);
        }
    }

    void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage -= 1;
            SetPage(currentPage);
        }
    }
    void NextPage()
    {
        if (currentPage < indicators.Count - 1)
        {
            currentPage += 1;
            SetPage(currentPage);
        }
    }

    public void SetPage(int pageIndex)
    {
        currentPage = pageIndex;
        if (webViewPrefab.WebView != null)
        {
            string html = languageManager.GetTutorialHtml(pageIndex);
            webViewPrefab.WebView.LoadHtml(html);
        }
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        for (int i = 0; i < indicators.Count; i++)
        {
            var img = indicators[i].GetComponent<Image>();
            img.sprite = (i == currentPage) ? activeIndicator : inactiveIndicator;
        }
    }
}