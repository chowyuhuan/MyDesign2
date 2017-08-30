using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Network;

public class BallUI : GUI_Window, IMCEventListener {
    public Camera uiCamera;

    public FrontSightUI FrontSight;

    public Button ShootButton;

    public BallInputPanel TouchInputPanelUI;

    public GUI_Tweener OptionsTween;
    public GUI_Tweener ExitTween;
    private bool isCollapseMenu = true;

    public GameObject Menu;

    public Text RoundNumber;

    public Text Score;

    public Text[] TargetNumbers;

    private UINotify _uiNotify = null;

    void Awake()
    {
        uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
    }

    void OnEnable()
    {
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_BATTLE_INPUT_DOWN, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_BATTLE_INPUT_UP, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_BALL_NEW_ROUNG, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_BALL_UPDATE_NUMBERS, this);
        MCEventCenter.instance.registerMCEventListener(MCEventType.UI_BALL_UPDATE_SCORE, this);
    }

    void OnDisable()
    {
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_BATTLE_INPUT_DOWN, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_BATTLE_INPUT_UP, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_BALL_NEW_ROUNG, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_BALL_UPDATE_NUMBERS, this);
        MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_BALL_UPDATE_SCORE, this);
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
            case MCEventType.UI_BALL_NEW_ROUNG:
                RoundNumber.text = evt.IntValue.ToString();
                break;
            case MCEventType.UI_BALL_UPDATE_NUMBERS:
                UpdateNumbers(evt.ListValue);
                break;
            case MCEventType.UI_BALL_UPDATE_SCORE:
                UpdateScore(evt.IntValue);
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

    private void UpdateNumbers(List<object> numbers)
    {
        for (int i = 0; i < TargetNumbers.Length; i++)
        {
            TargetNumbers[i].enabled = false;
        }

        int n = Mathf.Min(TargetNumbers.Length, numbers.Count);

        for (int i = 0; i < n; i++)
        {
            TargetNumbers[i].enabled = true;
            TargetNumbers[i].text = numbers[i].ToString(); 
        }
    }

    private void UpdateScore(int score)
    {
        Score.text = score.ToString();
    }
}
