using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using Network;

namespace NumberShoot
{
    public class GameManager : MonoBehaviour, IMCEventListener, BulletColliderResult
    {
        [HideInInspector]
        public NumberUI numberUI;

        public SceneConfig sceneConfig;

        public Player player;

        public RoundManager roundManager;

        public SaucerManager saucerManager;

        public ScoreBoard scoreBoard;

        public ItemClue itemClue;

        private AimController _aimController;
        
        private bool updateAim;

        public int wrongQuestionCount;

        private int wrongQuestionCounter;

        public float questionInterval;

        private bool isAnswerRight;

        private bool isGameOver;

        private int answerRightCounter;

        private bool isSkipSaucerRound;

        private bool isShowAnswer;

        public GameObject bonusEffect;

        void OnEnable()
        {
            Common.LogD("GameManager OnEnable");

            StartGame();
        }

        void OnDisable()
        {
            Common.LogD("GameManager OnDisable");

            EndGame();
        }

        void Update()
        {
            UpdateAim();
        }

        public void OnEvent(MCEvent evt)
        {
            switch (evt.Type)
            {
                case MCEventType.REQUEST_FIRE:
                    Fire();
                    break;
                case MCEventType.REQUEST_AIM_ROTATE:
                    Vector2 axis = (Vector2)evt.ObjectValue;
                    _aimController.AimRotate(axis.x, axis.y, false);
                    break;
                case MCEventType.GAME_INTERRUPT:
                    StopUpdateAim();
                    break;
                case MCEventType.UI_NUMBER_START_ANIMATION_FINISHED:
                    roundManager.QuestionStart();
                    break;
                case MCEventType.UI_NUMBER_GAME_OVER_ANIMATION_FINISHED:
                    ShowSettlement();
                    break;
                case MCEventType.UI_GAME_RESTART:
                    InitGame();
                    break;
                case MCEventType.UI_NUMBER_BONUS_TIME:
                    ShowBonusEffect(evt.BooleanValue);
                    break;
                case MCEventType.UI_NUMBER_GUIDE_OVER:
                    InitGame();
                    break;
            }
        }

        protected void StartGame()
        {
            MCEventCenter.instance.registerMCEventListener(MCEventType.REQUEST_FIRE, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.REQUEST_AIM_ROTATE, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.GAME_INTERRUPT, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_START_ANIMATION_FINISHED, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_GAME_OVER_ANIMATION_FINISHED, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.UI_GAME_RESTART, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_BONUS_TIME, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.UI_NUMBER_GUIDE_OVER, this);

            InitPlayer();
            if (!UIGuideNumberMode.CheckShow())
            {
                InitGame();
            }
        }

        protected void EndGame()
        {
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.REQUEST_FIRE, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.REQUEST_AIM_ROTATE, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.GAME_INTERRUPT, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_START_ANIMATION_FINISHED, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_GAME_OVER_ANIMATION_FINISHED, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_GAME_RESTART, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_BONUS_TIME, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.UI_NUMBER_GUIDE_OVER, this);
        }

        protected void InitGame()
        {
            InitUI();
            InitBullet();
            InitAim();
            InitScoreBoard();
            InitSaucerManager();
            InitRoundManager();
            InitWrongQuestionCount();
            InitItem();

            StartCoroutine(RoundNext(0));
            PlayStartAnimation();

            isGameOver = false;
            answerRightCounter = 0;
        }

        private void PlayStartAnimation()
        {
            MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.UI_NUMBER_START_ANIMATION));
        }

        private void InitUI()
        {
            numberUI = GUI_Manager.Instance.ShowWindowWithName("NumberUI", false) as NumberUI;
        }

        private void InitScoreBoard()
        {
            scoreBoard = new ScoreBoard();
            scoreBoard.ScoreBest = DataCenter.savedata.NumberModeRecord.GetBestRecord();
            scoreBoard.ScoreWeekBest = DataCenter.savedata.NumberModeRecord.GetWeekRecord();

            PlayerInfoManager.instance.CurrentPlayer.BestRecord = scoreBoard.ScoreBest;
            PlayerInfoManager.instance.CurrentPlayer.WeekRecord = scoreBoard.ScoreWeekBest;
        }

        private void InitWrongQuestionCount()
        {
            wrongQuestionCounter = 0;
        }

        private void InitItem()
        {
            BattleItemHelper.Init();
            itemClue.Number = (int)BattleItemHelper.GetItemCount((uint)BattleItemHelper.ItemId.Prompter);
        }

        private void InitBullet()
        {
            BulletCollider.Instance.result = this;

            player.PlayerGun.Reload();
            player.PlayerGun.Prepare();

            MCEvent bulletUpdateEvent = new MCEvent(MCEventType.PLAYER_BULLET_UPDATE);
            bulletUpdateEvent.IntValue = player.PlayerGun.currBulletNum;
            MCEventCenter.instance.dispatchMCEvent(bulletUpdateEvent);
        }

        private void InitPlayer()
        {
            PlayerInfoManager.instance.InitData();
            player.SetPlayerPosition();
            player.InitPlayerInfo(PlayerInfoManager.instance.CurrentPlayer);
            player.PullAnimate(false);
        }

        private void InitAim()
        {
            _aimController = new AimController(AimController.SimulateType.NONE);
            _aimController.UICamera = numberUI.uiCamera;
            _aimController.FrontSight = numberUI.FrontSight;
            _aimController.CurrentPlayer = player;

            updateAim = true;
        }

        private void UpdateAim()
        {
            if (updateAim)
            {
                _aimController.IKUpdate();
                _aimController.FrontSightUpdate(saucerManager.SaucerActive.ConvertAll<Saucer>(x => x.GetComponent<Saucer>()).ToArray());
            }
        }

        private void StopUpdateAim()
        {
            updateAim = false;
        }

        private void Fire()
        {
            if (_aimController.CanShoot())
            {
                _aimController.Shoot();
                _aimController.UpdateAndReload(true, false);
            }
        }

        private void ReturnBullet()
        {
            player.PlayerGun.currBulletNum += 1;
        }

        public void OnShootResult(AimTargetObject aimTarget, int hitLevel)
        {
            if (aimTarget == null)
            {
                return;
            }

            ReturnBullet();

            // Shoot at item clue
            if (aimTarget.GetComponent<ItemClue>() != null)
            {
                ItemClue itemClue = aimTarget.GetComponent<ItemClue>();
                if (itemClue.Number > 0)
                {
                    BattleItemHelper.UseItem((uint)BattleItemHelper.ItemId.Prompter);
                    itemClue.Number--;
                    isShowAnswer = true;
                    isSkipSaucerRound = true;
                    ClearAllSaucer();
                    isSkipSaucerRound = false;
                    StartCoroutine(RoundNext(questionInterval));
                }
            }
            // Shoot at saucer
            else if (aimTarget.GetComponent<Saucer>() != null)
            {
                Saucer saucer = aimTarget.GetComponent<Saucer>();

                int score = CalculateSaucerScore(saucer, hitLevel);

                if (saucerManager.IsBonusTime)
                {
                    scoreBoard.BonusSaucerScore(score);
                    ClearSaucer(aimTarget.gameObject);
                }
                else
                {
                    scoreBoard.SaucerScore(score);
                    roundManager.FillQuestion(saucer.Number);

                    if (roundManager.IsQuestionFinished())
                    {
                        if (roundManager.IsQuestionAnsweredCorrect())
                        {
                            scoreBoard.Combo += 1;
                            scoreBoard.CalculateQuestionScore();

                            isAnswerRight = true;
                            answerRightCounter++;
                            roundManager.RightAnswerEvent();
                        }

                        ClearAllSaucer();
                    }
                    else
                    {
                        ClearSaucer(aimTarget.gameObject);
                    }
                }
            }
        }

        private int CalculateSaucerScore(Saucer saucer, int hitLevel)
        {
            int score = 0;

            float saucerHitTime = Time.time - saucer.Time;
            if (hitLevel >= ScoreConfig.Instance.PerfectHitRays)
            {
                score = ScoreConfig.Instance.CalculateScore(ScoreConfig.Instance.ScorePerfectHit, saucerHitTime, false);
            }
            else
            {
                score = ScoreConfig.Instance.CalculateScore(ScoreConfig.Instance.ScoreNormalHit, saucerHitTime, false);
            }

            return score;
        }

        private void InitSaucerManager()
        {
            saucerManager.OnSaucerRoundFinished = OnSaucerRoundFinished;
            saucerManager.OnAllSaucerRoundFinished = OnAllSaucerRoundFinished;
            saucerManager.OnAllSaucerBonusFinished = OnAllSaucerBonusFinished;
            saucerManager.OnSaucerRoundAnswer = OnSaucerRoundAnswer;
        }

        private void ClearAllSaucer()
        {
            saucerManager.SetCurrentRoundFinished();
            saucerManager.RecycleAllSaucer();
        }

        private void ClearSaucer(GameObject saucer)
        {
            saucerManager.RecycleSaucer(saucer);
        }

        private void InitRoundManager()
        {
            roundManager.OnNewQuestion = OnNewQuestion;
            roundManager.OnNewRound = OnNewRound;
            roundManager.OnGameOver = OnGameOver;
            roundManager.InitNormalRound();
        }

        public void OnSaucerRoundFinished()
        {
            if (isSkipSaucerRound)
            {
                return;
            }

            if (!isAnswerRight)
            {
                roundManager.ResetQuestion();

                scoreBoard.ResetSaucerScore();
                scoreBoard.Combo = 0;
            }
        }

        public void OnAllSaucerRoundFinished()
        {
            if (isSkipSaucerRound)
            {
                return;
            }

            if (!isAnswerRight)
            {
                wrongQuestionCounter++;

                roundManager.WrongAnswerEvent();

                scoreBoard.Combo = 0;
            }

            if (!isGameOver)
            {
                roundManager.RoundEnd();
                StartCoroutine(RoundNext(questionInterval));
            }
        }

        public void OnAllSaucerBonusFinished()
        {
            StartCoroutine(RoundNext(questionInterval));
        }

        public void OnSaucerRoundAnswer(int[] answer)
        {
            MCEvent answerEvent = new MCEvent(MCEventType.UI_NUMBER_SHOW_ANSWER);
            answerEvent.BooleanValue = isShowAnswer;
            answerEvent.StringValue = roundManager.FillQuestionByAnswer(answer);
            MCEventCenter.instance.dispatchMCEvent(answerEvent);
            isShowAnswer = false;
        }

        public void OnNewQuestion(float difficulty)
        {
            isAnswerRight = false;
            scoreBoard.NewQuestion(difficulty);
            scoreBoard.ResetSaucerScore();
        }

        public void OnNewRound()
        {
            scoreBoard.Combo = 0;
        }

        public void OnGameOver()
        {
            isGameOver = true;
            ClearAllSaucer();
            EnterSettlement();
            Common.LogD("Game Over");
        }

        IEnumerator RoundNext(float interval)
        {
            yield return new WaitForSeconds(interval);

            if (!roundManager.Next())
            {
                EnterSettlement();
                Common.LogD("Game Finished");
            }
        }

        private void EnterSettlement()
        {
            StopUpdateAim();

            SaveBestScore();
            CalculateReward();
            RequestGameScore();

            MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.UI_NUMBER_GAME_OVER));
        }

        public void ShowSettlement()
        {
            GUI_Manager.Instance.HideWindowWithName("NumberUI");
            GUI_Manager.Instance.ShowWindowWithName("PVECommonSettlement", false);
        }

        private void SaveBestScore()
        {
            if (scoreBoard.ScoreBestDirty)
            {
                DataCenter.savedata.NumberModeRecord.SetBestRecord(scoreBoard.ScoreBest);
                DataCenter.savedata.NumberModeRecord.SetWeekRecord(scoreBoard.ScoreWeekBest);
                DataCenter.Save();
            }
        }

        private void RequestGameScore()
        {
            gateproto.ResultReq request = new gateproto.ResultReq();
            request.session_id = DataCenter.sessionId;
            request.score = (uint)scoreBoard.ScoreWeekBest;
            request.room_type = 501;
            request.collect_dice_count = (uint)answerRightCounter;
            BattleItemHelper.PushItems(request.consume_item_list);
            NetworkManager.SendRequest(ProtocolDataType.TcpPersistent, request);
        }

        private void CalculateReward()
        {
            int exp = (int)Math.Round(scoreBoard.ScoreTotal / 150.0f, MidpointRounding.AwayFromZero);
            int gold = exp * 3;
            PlayerInfoManager.instance.AddExpCount(exp);
            PlayerInfoManager.instance.AddGoldCount(gold);
            PlayerInfoManager.instance.CurrentPlayer.Score = scoreBoard.ScoreTotal;
        }

        private void ShowBonusEffect(bool show)
        {
            bonusEffect.SetActive(show);
            ParticleSystem particle = bonusEffect.GetComponent<ParticleSystem>();
            if (show)
            {
                particle.Play();
            }
            else
            {
                particle.Stop();
            }
        }
    }
}
