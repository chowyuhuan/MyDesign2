using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using Network;
using NumberShoot;

public class NumberUI : GUI_Window, IMCEventListener {
    public Camera uiCamera;

    public FrontSightUI FrontSight;

    public Button ShootButton;

    public NumberInputPanel NumberInputPanelUI;

    public GUI_Tweener OptionsTween;
    public GUI_Tweener ExitTween;
    private bool isCollapseMenu = true;

    public GameObject Menu;

    public Text RoundNumber;

    public GameObject[] Life;
    public Text LifeNumber;

    public GameObject QuestionRoot;
    //public GameObject QuestionIce; 
    public GameObject[] QuestionElement;

    public Text QuestionCounter;
    public Text QuestionNum;

    public Text Degree;

    public GameObject RightFlag;
    public GameObject WrongFlag;

    public GameObject RightBackGround;
    public GameObject WrongBackGround;

    public Text SaucerScore;
    public Text TotalScore;

    public GameObject ComboRoot;
    public Text Combo;

    public Text BestScore;
    public Text WeekBestScore;

    public GameObject BonusTime;
    public GameObject GameOver;

    public GameObject QuestionPanel;
    public GameObject TimePanel;

    public RectTransform QuestionPanelPos5;
    public RectTransform QuestionPanelPos7;

    public GameObject[] ComboEffect;

    public Text TimeAdd;

    public Text Answer;

    private GameObject CurrentComboEffect;

    private UINotify _uiNotify = null;

    void Awake()
    {
        uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
    }

    void OnEnable()
    {
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_BATTLE_INPUT_DOWN, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_BATTLE_INPUT_UP, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_NEW_ROUNG, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_ANSWER, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_UPDATE_QUESTION, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_QUESTION_ICE, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_SAUCER_SCORE, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_QUESTION_SCORE, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_UPDATE_COMBO, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_BEST_SCORE, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_WEEK_BEST_SCORE, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_GAME_OVER, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_BONUS_TIME, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_TIME_ADD, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_SHOW_ANSWER, this);
    }

    void OnDisable()
    {
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_BATTLE_INPUT_DOWN, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_BATTLE_INPUT_UP, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_NEW_ROUNG, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_ANSWER, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_UPDATE_QUESTION, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_QUESTION_ICE, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_SAUCER_SCORE, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_QUESTION_SCORE, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_UPDATE_COMBO, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_BEST_SCORE, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_WEEK_BEST_SCORE, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_GAME_OVER, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_BONUS_TIME, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_TIME_ADD, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_SHOW_ANSWER, this);
    }

    public void OnClickFire()
    {
        MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.REQUEST_FIRE));
    }

    public void OnClickPull()
    {
        MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.REQUEST_PULL));
    }

    void IMCEventListener.OnEvent(MCEvent evt)
    {
        PlayerInfoManager piManager = PlayerInfoManager.instance;
        switch (evt.Type)
        {
            case MCEventType.UI_BATTLE_INPUT_DOWN:
                if (!this.isCollapseMenu)
                {
                    this.OnClickMenu();
                }
                break;
            case MCEventType.UI_BATTLE_INPUT_UP:
                break;
            case MCEventType.UI_NUMBER_NEW_ROUNG:
                OnEventNewRound(evt.IntValue, (int)evt.DictValue["QuestionNum"]);
                break;
            case MCEventType.UI_NUMBER_ANSWER:
                OnEventAnswer(evt.BooleanValue);
                break;
            case MCEventType.UI_NUMBER_UPDATE_QUESTION:
                OnEventUpdateQuestion(evt.IntValue, evt.FloatValue, evt.ListValue, evt.BooleanValue);
                break;
            case MCEventType.UI_NUMBER_QUESTION_ICE:
                OnEventQuestionIce(evt.BooleanValue);
                break;
            case MCEventType.UI_NUMBER_SAUCER_SCORE:
                OnEventUpdateSaucerScore(evt.IntValue);
                break;
            case MCEventType.UI_NUMBER_QUESTION_SCORE:
                OnEventUpdateQuestionScore(evt.IntValue);
                break;
            case MCEventType.UI_NUMBER_UPDATE_COMBO:
                OnEventUpdateCombo(evt.IntValue);
                break;
            case MCEventType.UI_NUMBER_BEST_SCORE:
                OnEventUpdateBestScore(evt.IntValue);
                break;
            case MCEventType.UI_NUMBER_WEEK_BEST_SCORE:
                OnEventUpdateWeekBestScore(evt.IntValue);
                break;
            case MCEventType.UI_NUMBER_GAME_OVER:
                OnEventGameOver();
                break;
            case MCEventType.UI_NUMBER_BONUS_TIME:
                OnEventBonusTime(evt.BooleanValue);
                break;
            case MCEventType.UI_NUMBER_TIME_ADD:
                OnEventTimeAdd(evt.FloatValue);
                break;
            case MCEventType.UI_NUMBER_SHOW_ANSWER:
                OnEventShowAnswer(evt.BooleanValue, evt.StringValue);
                break;
        }
    }

    void OnExit()
    {
        BattleTempData.Instance.IsSelfQuit = true;
        MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.GAME_INTERRUPT));
        this.HideWindow();
        GUI_Manager.Instance.PushWndToWaitingQueue("LobbyUI");
        BattleTempData.Instance.ClearData();
        PL_Manager.Instance.LoadSceneAsync("Lobby", false);
    }

    public void OnClickMenu()
    {
        OptionsTween.Play(this.isCollapseMenu);
        ExitTween.Play(this.isCollapseMenu);
        this.isCollapseMenu = !this.isCollapseMenu;
    }

    public void OnClickExit()
    {
        //this.ExitBattle();
        _uiNotify = new UINotify.Builder()
            .SetTitle("WARNING")
            .SetContent("          Are you sure to leave and lose the game?")
            .SetOnConfirm((sender) =>
            {
                sender.Close();
                _uiNotify = null;
                this.OnExit();
            })
            .SetOnReturn((sender) => { 
                sender.Close();
                _uiNotify = null;
            }).Get();
    }

    public void OnClickOptions()
    {
        GUI_Manager.Instance.ShowWindowWithName("SettingUI", false);
    }

    private void OnEventUpdateSaucerScore(int score)
    {
        SaucerScore.text = score.ToString();
    }

    private void OnEventUpdateQuestionScore(int score)
    {
        SaucerScore.text = score.ToString();
        TotalScore.text = (Convert.ToInt32(TotalScore.text) + score).ToString();
    }

    private void OnEventUpdateCombo(int combo)
    {
        combo = Mathf.Max(0, combo - 1);
        Combo.text = combo.ToString();
        if (combo > 0)
        {
            //ComboRoot.SetActive(true);
            GUI_TweenerUtility.ResetAndPlayTweeners(Combo.gameObject);
        }
        else
        {
            //ComboRoot.SetActive(false);
        }

        CurrentComboEffect = ComboEffect[combo];
    }

    private void ComboEffectFinished()
    {
        CurrentComboEffect.SetActive(false);
    }

    private void OnEventUpdateBestScore(int score)
    {
        BestScore.text = score.ToString();
    }

    private void OnEventUpdateWeekBestScore(int score)
    {
        WeekBestScore.text = score.ToString();
    }

    private void OnEventNewRound(int roundNum, int questionNum)
    {
        RoundNumber.text = roundNum.ToString();
        QuestionNum.text = questionNum.ToString();
    }

    private void ResetResultShow()
    {
        RightFlag.SetActive(false);
        RightBackGround.SetActive(false);
        WrongFlag.SetActive(false);
        WrongBackGround.SetActive(false);
    }

    private void ReduceLife()
    {
        foreach (GameObject hp in Life)
        {
            if (hp.activeSelf)
            {
                hp.SetActive(false);
                LifeNumber.text = (Convert.ToInt32(LifeNumber.text) - 1).ToString();
                break;
            }
        }
    }

    private void OnEventAnswer(bool result)
    {
        if (result)
        {
            RightFlag.SetActive(true);
            RightBackGround.SetActive(true);

            CurrentComboEffect.SetActive(true);
            GUI_TweenerUtility.ResetAndPlayTweeners(CurrentComboEffect, ComboEffectFinished);
        }
        else
        {
            WrongFlag.SetActive(true);
            WrongBackGround.SetActive(true);

            ReduceLife();
        }
    }

    private string ConvertElemet(string element)
    {
        if (element == "*")
        {
            return "×";
        }
        else if (element == "/")
        {
            return "÷";
        }
        else
        {
            return element;
        }
    }

    private void SetElement(GameObject gameObject, string element, bool isFill)
    {
        gameObject.SetActive(true);

        GameObject showBG = gameObject.transform.FindChild("Show_BG").gameObject;
        GameObject unKnownBG = gameObject.transform.FindChild("Unknown_BG").gameObject;
        GameObject fillBG = gameObject.transform.FindChild("Fill_BG").gameObject;

        showBG.SetActive(false);
        unKnownBG.SetActive(false);
        fillBG.SetActive(false);

        if (element == QuestionGenerator.ElementBlank)
        {
            unKnownBG.SetActive(true);
        }
        else if (Regex.IsMatch(element, @"^\d*$") && isFill)
        {
            fillBG.SetActive(true);
            Text number = fillBG.transform.FindChild("Unknown_Number").gameObject.GetComponent<Text>();
            number.text = element;
        }
        else
        {
            showBG.SetActive(true);
            Text number = showBG.transform.FindChild("Show_Number").gameObject.GetComponent<Text>();
            number.text = ConvertElemet(element);
        }
    }

    private void ResetQusetionElemet()
    {
        foreach (GameObject gameObject in QuestionElement)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEventQuestionIce(bool show)
    {
        //QuestionIce.SetActive(show);
    }

    private void OnEventUpdateQuestion(int counter, float difficulty, List<object> elements, bool showEffect)
    {
        ResetResultShow();
        ResetQusetionElemet();
        SetQuestionPanelPosition(elements.Count);

        QuestionCounter.text = (Convert.ToInt32(QuestionNum.text) - counter).ToString();
        Degree.text = difficulty.ToString();

        int n = QuestionElement.Length - 1;
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            SetElement(QuestionElement[n--], elements[i].ToString(), i != elements.Count - 1);           
        }

        if (showEffect)
        {
            QuestionRoot.SetActive(true);
            GUI_TweenerUtility.ResetAndPlayTweeners(QuestionRoot);
        }
    }

    private void SetQuestionPanelPosition(int questionLength)
    {
        if (questionLength == 5)
        {
            QuestionPanel.GetComponent<RectTransform>().position = QuestionPanelPos5.position;
            QuestionPanel.GetComponent<RectTransform>().sizeDelta = QuestionPanelPos5.rect.size;
        }
        else if (questionLength == 7)
        {
            QuestionPanel.GetComponent<RectTransform>().position = QuestionPanelPos7.position;
            QuestionPanel.GetComponent<RectTransform>().sizeDelta = QuestionPanelPos7.rect.size;
        }
    }

    private void OnEventGameOver()
    {
        GameOver.SetActive(true);
        GUI_TweenerUtility.ResetAndPlayTweeners(GameOver, GameOverAnimationFinished);
    }

    private void GameOverAnimationFinished()
    {
        GameOver.SetActive(false);
        MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.UI_NUMBER_GAME_OVER_ANIMATION_FINISHED));
    }

    private void OnEventBonusTime(bool flag)
    {
        if (flag)
        {
            BonusTime.SetActive(true);
            GUI_TweenerUtility.ResetAndPlayTweeners(BonusTime, BonusTimeAnimationFinished);
        }
        QuestionPanel.SetActive(!flag);
        TimePanel.SetActive(!flag);
    }

    private void BonusTimeAnimationFinished()
    {
        BonusTime.SetActive(false);
    }

    private void OnEventTimeAdd(float time)
    {
        TimeAdd.text = "+" + time;
        TimeAdd.gameObject.SetActive(true);
        GUI_TweenerUtility.ResetAndPlayTweeners(TimeAdd.gameObject, TimeAddAnimationFinished);
    }

    private void TimeAddAnimationFinished()
    {
        TimeAdd.gameObject.SetActive(false);
    }

    private void OnEventShowAnswer(bool show, string answer)
    {
        Answer.gameObject.SetActive(show);
        Answer.text = answer;
    }
}
