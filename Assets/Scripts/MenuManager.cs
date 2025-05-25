using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private APIManager API;
    [SerializeField] private MainGame game;
    [SerializeField] private AudioManager Audio;

    [SerializeField] private GameObject mainGameScreenUI;
    public string playerCurrency = "IDR";
    public string playerLanguage = "id";
    [SerializeField]
    private long[] chipValues = new long[] {
        1000,
        2000,
        5000,
        10000,
        20000,
        50000,
        100000,
        500000,
        1000000
    };
    int selectedChipValueIndex = 0;
    float minBet = 1000;
    float maxBet = 50000;


    [SerializeField] private GameObject decreaseBetButton, increaseBetButton;
    [SerializeField] private TextMeshProUGUI txtBet;
    private float currentBet = 0;

    [SerializeField] private int[] coinCount = new int[] {1, 5};
    [SerializeField] private GameObject[] coinButtons;
    [SerializeField] private SkeletonGraphic[] coinButtonSpine;
    [SerializeField] private TextMeshProUGUI[] coinButtonTexts;

    int selectedCoinCount;

    [SerializeField] private long totalPlayerBalance = 500000;
    [SerializeField] private TextMeshProUGUI txtPlayerBalance;

    [Header("Launch Indikator")]
    [SerializeField] private GameObject launchButton;
    [SerializeField] private GameObject launchButtonIndicator;
    [SerializeField] private Transform[] indicatorArrow;
    [SerializeField] private Transform[] indicatorArrowPos;
    IEnumerator launchHintCoroutine;
    public float idleTimeToShowHint = 3.5f;
    bool CanPressLaunch = true;

    [SerializeField] private GameObject autoPlayButton;
    [SerializeField] private SkeletonGraphic autoPlayButtonSpine;
    public bool isAutoPlay = false;

    [Header("Title Screen")]
    [SerializeField] private GameObject titleScreenUI;
    [SerializeField] private SkeletonGraphic titleScreenSpine;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject howToPlayButton;

    [SerializeField] private TutorialDisplay tutorialDisplay;

    IEnumerator insertCoinsCoroutine;

    [Header("Menu Option Buttons")]
    [SerializeField] private GameObject menuButton;
    [SerializeField] private Transform[] menuButtonPos;
    [SerializeField] private Transform menuButtonGroup;
    [SerializeField] private GameObject[] subMenuButtons;
    bool showMenu = false;
    Vector3 originalMenuOptionButtonScale;

    [Header("Audio Menu UI")]
    [SerializeField] private GameObject AudioMenuUI;
    [SerializeField] private GameObject bgmToogle, sfxToogle;

    [Header("History Menu UI")]
    [SerializeField] private GameObject HistoryUI;
    [SerializeField] private TextMeshProUGUI historyBetTitle, historyResultTitle;
    [SerializeField] private List<Transform> historyRows;
    bool canOpenHistoryAndExit = true;

    [Header("Message Info UI")]
    [SerializeField] private GameObject messageInfoUI;
    [SerializeField] private TextMeshProUGUI txtMessageInfo;
    [SerializeField] private TextMeshProUGUI txtNoInternet;

    private void Start()
    {
        CloseAllSubMenu();
        mainGameScreenUI.SetActive(false);
        titleScreenUI.SetActive(true);
        StartCoroutine(PlayTitleScreenAnimation());
        ShowButtonPlayAndTutorial(false);
        launchButtonIndicator.SetActive(false);

        selectedCoinCount = coinCount[0];
        currentBet = chipValues[selectedChipValueIndex];
        txtBet.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet, playerCurrency);

        txtPlayerBalance.text = playerCurrency + " " + StringHelper.MoneyFormat(totalPlayerBalance, playerCurrency);

        for (int i = 0; i < coinButtonTexts.Length; i++)
        {
            coinButtonTexts[i].text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet * coinCount[i], playerCurrency);
        }

        AssignTitleScreenButtons();
        AssignMenuButtons();
    }


    public void SetUserData(UserDataResponse response)
    {
        chipValues = response.data.game.chip_base;
        currentBet = chipValues[0];
        minBet = response.data.game.limit_bet.minimal;
        maxBet = response.data.game.limit_bet.maximal;
        playerCurrency = response.data.player.player_currency;

        playerLanguage = response.data.player.player_language.ToLower();
        LanguageManager.Instance.SetLanguage(playerLanguage);
        SetHistoryTitleText();
        SetCoinButtons();
        SelectCoinCount(0);
        ChangeBet(false);

        ChangePlayerBallance(response.data.player.player_balance);
        Audio.SaveAndSetAudioSetting(response.data.game.sounds, bgmToogle, sfxToogle);
    }

    public void ChangePlayerBallance(string balance)
    {
        totalPlayerBalance = Convert.ToInt64(balance);
        txtPlayerBalance.text = playerCurrency + " " + StringHelper.MoneyFormat(totalPlayerBalance, playerCurrency);
    }

    public void ShowButtonPlayAndTutorial(bool isShow = true)
    {
        startButton.SetActive(isShow);
        howToPlayButton.SetActive(isShow);
    }

    IEnumerator PlayTitleScreenAnimation()
    {
        SpineHelper.PlayAnimation(titleScreenSpine, "start", false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(titleScreenSpine, "start"));
        SpineHelper.PlayAnimation(titleScreenSpine, "idle", true);
    }

    void AssignTitleScreenButtons()
    {
        EventTrigger triggerStart = startButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryStart = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryStart.callback.AddListener((data) => {
            Audio.PlaySFX(7);
            titleScreenUI.SetActive(false);
            mainGameScreenUI.SetActive(true);
        });
        triggerStart.triggers.Add(entryStart);

        EventTrigger triggerInfo = howToPlayButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryInfo = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryInfo.callback.AddListener((data) => { OnSubMenuButton(3); });
        triggerInfo.triggers.Add(entryInfo);
    }

    void AssignMenuButtons()
    {
        SetBetButtons();
        SetCoinButtons();
        AssignLaunchButton();
        AssignAutoPlayButtons();
        AssignMenuOptionButtons();
        AssignAudioSettingButtons();

        EnableMenuButtons(true);
        SelectCoinCount(0);

    }

    void AssignMenuOptionButtons()
    {
        originalMenuOptionButtonScale = menuButton.transform.localScale;
        EventTrigger eventTriggerMenu = menuButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryMenu = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryMenu.callback.AddListener((data) => { OpenSubMenu(); });
        eventTriggerMenu.triggers.Add(entryMenu);

        for (int i = 0; i < subMenuButtons.Length; i++)
        {
            int index = i;
            EventTrigger eventTrigger = subMenuButtons[index].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) => { OnSubMenuButton(index); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void OpenSubMenu()
    {
        menuButton.transform.localScale = originalMenuOptionButtonScale;
        menuButton.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.15f, 1, 1).SetEase(Ease.Linear);
        showMenu = !showMenu;
        Audio.PlaySFX(showMenu ? 9 : 10);
        ShowMenuButtons();
    }

    void AssignAudioSettingButtons()
    {
        EventTrigger eventTriggerBgm = bgmToogle.AddComponent<EventTrigger>();
        EventTrigger.Entry entryBgm = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryBgm.callback.AddListener((data) => {
            Audio.ToogleBgm();
            Audio.SetAudioSetting(bgmToogle, sfxToogle, true);
        });
        eventTriggerBgm.triggers.Add(entryBgm);


        EventTrigger eventTriggerSfx = sfxToogle.AddComponent<EventTrigger>();
        EventTrigger.Entry entrySfx = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entrySfx.callback.AddListener((data) => {
            Audio.ToogleSfx();
            Audio.SetAudioSetting(bgmToogle, sfxToogle, true);
        });
        eventTriggerSfx.triggers.Add(entrySfx);
    }


    public void ShowMenuButtons()
    {
        menuButtonGroup.DOMoveY(menuButtonPos[showMenu ? 1 : 0].position.y, 0.15f).SetEase(Ease.Linear);
    }

    void OnSubMenuButton(int index, bool playSfx = true)
    {
        if(playSfx)
            Audio.PlaySFX(7);
        
        if (index == 0)
        {
            BackToTitleScreen();
        }
        else if (index == 1)
        {
            AudioMenuUI.SetActive(true);
        }
        else if (index == 2)
        {
            OpenHistory();
        }
        else if (index == 3)
        {
            tutorialDisplay.OpenTutorial();
        }
        else
        {
            if (InternetConnectivity.IsConnected)
                CloseAllSubMenu();
        }
    }

    void BackToTitleScreen()
    {
        if (!canOpenHistoryAndExit)
            return;
        titleScreenUI.SetActive(true);
        mainGameScreenUI.SetActive(false);
        ForceCloseMenuOptionButtons();
    }

    void ForceCloseMenuOptionButtons()
    {
        showMenu = false;
        ShowMenuButtons();
    }

    void CloseAllSubMenu()
    {
        AudioMenuUI.SetActive(false);
        OpenHistory(false);
        //tutorialDisplay.OpenTutorial(false);
        messageInfoUI.SetActive(false);
    }

    void OpenHistory(bool isOpen = true)
    {
        if (!canOpenHistoryAndExit)
            return;
        if (isOpen)
        {
            StartCoroutine(API.GetHistoryDataIE(PassingHistoryData));
        }
        else
        {
            HistoryUI.SetActive(false);
        }
    }

    void PassingHistoryData(HistoryResponse response)
    {
        SetHistoryTitleText();
        historyRows.ForEach(x => x.gameObject.SetActive(false));
        List<HistoryData> historyList = response.data;
        if (historyList.Count > 0)
        {
            for (int i = 0; i < historyList.Count; i++)
            {
                int index = i;
                HistoryData data = historyList[i];
                historyRows[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = data.created_date.Replace(" ", "\n");
                historyRows[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = StringHelper.MoneyFormat(data.data.total_amount.ToString("G30"), playerCurrency);
                historyRows[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = StringHelper.MoneyFormat(data.data.total_win.ToString("G30"), playerCurrency);
                historyRows[i].gameObject.SetActive(true);
            }
        }
        HistoryUI.SetActive(true);
    }

    void SetHistoryTitleText()
    {
        historyBetTitle.text = LanguageManager.Instance.GetLabel("txt_bet_history", "Bet") + "\n" + "(" + playerCurrency + ")";
        historyResultTitle.text = LanguageManager.Instance.GetLabel("txt_result_history", "Result") + "\n" + "(" + playerCurrency + ")";
    }

    void SetBetButtons()
    {
        EventTrigger triggerDecrease = decreaseBetButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryDecrease = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryDecrease.callback.AddListener((data) => { ChangeBet(false); });
        triggerDecrease.triggers.Add(entryDecrease);

        EventTrigger triggerIncrease = increaseBetButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryIncrease = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryIncrease.callback.AddListener((data) => { ChangeBet(true); });
        triggerIncrease.triggers.Add(entryIncrease);
    }

    void SetCoinButtons()
    {
        for (int i = 0; i < coinButtons.Length; i++)
        {
            int index = i;
            SpineHelper.PlayAnimation(coinButtonSpine[i], "OFF", true);
            coinButtons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                coinCount[index].ToString() + " " +
                (coinCount[index] > 1
                    ? LanguageManager.Instance.GetLabel("txt_coin_plural", "Coins")
                    : LanguageManager.Instance.GetLabel("txt_coin_single", "Coin"));

            EventTrigger existingTrigger = coinButtons[index].GetComponent<EventTrigger>();
            if (existingTrigger != null)
            {
                Destroy(existingTrigger);
            }
            EventTrigger trigger = coinButtons[index].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) => {
                Audio.PlaySFX(8);
                SelectCoinCount(index);
            });
            trigger.triggers.Add(entry);
        }
    }

    void SelectCoinCount(int index)
    {
        if (isAutoPlay)
            return;
        SetCoinCount(index);
        if (!launchButtonIndicator.activeInHierarchy)
        {
            ShowLaunchButtonHint();
        }
    }

    void LaunchCoins()
    {
        if (totalPlayerBalance < Convert.ToInt64(currentBet))
            return;

        if (isAutoPlay)
            return;
        StopLaunchButtonHint();
        ForceCloseMenuOptionButtons();
        InsertCoin();
    }

    void SetCoinCount(int index)
    {
        ResetCoinButtonsColor();
        SpineHelper.PlayAnimation(coinButtonSpine[index], "ON", true);
        selectedCoinCount = coinCount[index];
    }

    public void ResetCoinButtonsColor()
    {
        for (int i = 0; i < coinButtons.Length; i++)
        {
            SpineHelper.PlayAnimation(coinButtonSpine[i], "OFF", true);
        }
    }

    void ChangeBet(bool isIncrease)
    {
        Audio.PlaySFX(7);
        int prevChip = selectedChipValueIndex;
        selectedChipValueIndex += isIncrease ? 1 : -1;
        selectedChipValueIndex = Mathf.Clamp(selectedChipValueIndex, 0, chipValues.Length - 1);
        currentBet = chipValues[selectedChipValueIndex];
        if (currentBet < minBet || currentBet > maxBet)
        {
            selectedChipValueIndex = prevChip;
            currentBet = chipValues[selectedChipValueIndex];
            return;
        }
        currentBet = Mathf.Max(currentBet, 0);
        txtBet.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet, playerCurrency);
        for (int i = 0; i < coinButtonTexts.Length; i++)
        {
            coinButtonTexts[i].text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet * coinCount[i], playerCurrency);
        }
    }

    public void ShowLaunchButtonHint(float idleTime = 0.5f)
    {
        StopLaunchButtonHint();
        launchHintCoroutine = ShowLaunchButtonHintIE(idleTime);
        StartCoroutine(launchHintCoroutine);
    }
    IEnumerator ShowLaunchButtonHintIE(float idleTime = 0.5f)
    {
        yield return new WaitForSeconds(idleTime);
        launchButtonIndicator.SetActive(true);
        indicatorArrow[0].transform.position = indicatorArrowPos[0].position;
        indicatorArrow[1].transform.position = indicatorArrowPos[1].position;
        float delay = 0.5f;
        while (true)
        {
            indicatorArrow[0].DOMove(indicatorArrowPos[2].position, delay).SetEase(Ease.Linear);
            indicatorArrow[1].DOMove(indicatorArrowPos[3].position, delay).SetEase(Ease.Linear);
            yield return new WaitForSeconds(delay * 1.5f);
            indicatorArrow[0].DOMove(indicatorArrowPos[0].position, delay).SetEase(Ease.Linear);
            indicatorArrow[1].DOMove(indicatorArrowPos[1].position, delay).SetEase(Ease.Linear);
            yield return new WaitForSeconds(delay);
        }
    }

    void StopLaunchButtonHint()
    {
        if(launchHintCoroutine != null)
        {
            StopCoroutine(launchHintCoroutine);
            launchHintCoroutine = null;
        }
        launchButtonIndicator.SetActive(false);
    }

    void AssignLaunchButton()
    {
        EventTrigger eventTrigger1 = launchButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entry1 = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry1.callback.AddListener((data) => { LaunchCoins(); });
        eventTrigger1.triggers.Add(entry1);
    }

    void AssignAutoPlayButtons()
    {
        EventTrigger eventTrigger1 = autoPlayButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entry1 = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry1.callback.AddListener((data) => { SetAutoPlay(); });
        eventTrigger1.triggers.Add(entry1);
    }

    public void InsertCoin()
    {
        if (totalPlayerBalance > Convert.ToInt64(currentBet))
        {
            InsertCoins(selectedCoinCount);
        }
    }

    public void SetAutoPlay()
    {
        isAutoPlay = !isAutoPlay;
        SpineHelper.PlayAnimation(autoPlayButtonSpine, isAutoPlay ? "ON" : "OFF", true);

        if (isAutoPlay)
        {
            StopLaunchButtonHint();
            InitiateAutoPlay();
        }
        else
        {
            if (!game.isActiveSession)
            {
                ShowLaunchButtonHint(idleTimeToShowHint);
            }
        }
    }

    public void InitiateAutoPlay()
    {
        InsertCoin();
        StopLaunchButtonHint();
    }


    public void InsertCoins(int count)
    {
        if (!CanPressLaunch)
            return;
        EnableMenuButtons(false);

        totalPlayerBalance -= Convert.ToInt64(currentBet * count);
        txtPlayerBalance.text = playerCurrency + " " + StringHelper.MoneyFormat(totalPlayerBalance, playerCurrency);
        if(insertCoinsCoroutine != null)
        {
            StopCoroutine(insertCoinsCoroutine);
            insertCoinsCoroutine = null;
        }
        insertCoinsCoroutine = game.InsertCoinIE(count);
        StartCoroutine(insertCoinsCoroutine);
    }

    public void EnableMenuButtons(bool enable = true)
    {
        canOpenHistoryAndExit = enable;
        subMenuButtons[0].GetComponent<Image>().color = enable ? Color.white : Color.gray;
        subMenuButtons[2].GetComponent<Image>().color = enable ? Color.white : Color.gray;
        CanPressLaunch = enable;
        decreaseBetButton.GetComponent<EventTrigger>().enabled = enable;
        increaseBetButton.GetComponent<EventTrigger>().enabled = enable;
        for (int i = 0; i < coinButtons.Length; i++)
        {
            coinButtons[i].GetComponent<EventTrigger>().enabled = enable;
        }
    }

    public void AddPlayerBalance(float value)
    {
        totalPlayerBalance += Convert.ToInt64(value);
        txtPlayerBalance.text = playerCurrency + " " + StringHelper.MoneyFormat(totalPlayerBalance, playerCurrency);
    }

    public float GetCurrentBet()
    {
        return currentBet;
    }

    public void ShowMessageInfo(string message = "")
    {
        txtMessageInfo.text = message;
        messageInfoUI.SetActive(true);
    }

    private void OnEnable()
    {
        InternetConnectivity.OnConnectivityChanged += OnInternetChange;
        if (!InternetConnectivity.IsConnected)
        {
            ShowMessageInfo("");
            txtNoInternet.gameObject.SetActive(true);
        }
        else
        {
            txtNoInternet.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        InternetConnectivity.OnConnectivityChanged -= OnInternetChange;
    }

    private void OnInternetChange(bool isOnline)
    {
        API.Log(isOnline ? "Connected to Internet" : "Disconnected from Internet");
        if (!isOnline)
        {
            ShowMessageInfo("");
            txtNoInternet.gameObject.SetActive(true);
        }
        else
        {
            txtNoInternet.gameObject.SetActive(false);
            OnSubMenuButton(4, false); // close all menu windows including message box
        }
    }
}
