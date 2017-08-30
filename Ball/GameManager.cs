using UnityEngine;
using System.Linq;

namespace BALL
{
    public class GameManager : MonoBehaviour, IMCEventListener, BulletColliderResult
    {
        [HideInInspector]
        public BallUI ballUI;

        public SceneConfig sceneConfig;

        public Player player;

        public RoundManager roundManager;

        public BallManager ballManager;

        public ScoreBoard scoreBoard;

        private AimController _aimController;

        private bool updateAim;

        void OnEnable()
        {
            Common.LogD("BallManager OnEnable");

            StartGame();
        }

        void OnDisable()
        {
            Common.LogD("BallManager OnDisable");

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
            }
        }

        protected void StartGame()
        {
            MCEventCenter.instance.registerMCEventListener(MCEventType.REQUEST_FIRE, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.REQUEST_AIM_ROTATE, this);
            MCEventCenter.instance.registerMCEventListener(MCEventType.GAME_INTERRUPT, this);

            InitUI();
            InitBullet();
            InitPlayer();
            InitAim();

            Invoke("RoundNext", 1.0f);
        }

        protected void EndGame()
        {
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.REQUEST_FIRE, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.REQUEST_AIM_ROTATE, this);
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.GAME_INTERRUPT, this);
        }

        private void InitUI()
        {
            ballUI = GUI_Manager.Instance.ShowWindowWithName("BallUI", false) as BallUI;
        }

        private void InitBullet()
        {
            BulletCollider.Instance.result = this;
        }

        private void InitPlayer()
        {
            PlayerInfoManager.instance.InitData();
            player.SetPlayerPosition();
            player.InitPlayerInfo(PlayerInfoManager.instance.CurrentPlayer);
            player.PullAnimate(false);
            player.PlayerGun.Prepare();
        }

        private void InitAim()
        {
            _aimController = new AimController(AimController.SimulateType.NONE);
            _aimController.UICamera = ballUI.uiCamera;
            _aimController.FrontSight = ballUI.FrontSight;
            _aimController.CurrentPlayer = player;

            updateAim = true;
        }

        private void UpdateAim()
        {
            if (updateAim)
            {
                _aimController.IKUpdate();
                _aimController.FrontSightUpdate(ballManager.BallActive.ConvertAll<Ball>(x => x.GetComponent<Ball>()).ToArray());
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

        public void OnShootResult(AimTargetObject aimTarget, int hitLevel)
        {
            if (aimTarget == null)
            {
                return;
            }

            Ball ball = aimTarget.GetComponent<Ball>();

            if (roundManager.CurrentTargetNumber == ball.Number)
            {
                scoreBoard.CalculateScore();
                ballManager.RecycleBall(aimTarget.gameObject);
                ballManager.ReplenishBall();
                RoundNext();
            }
            else
            {
                ball.RefreshNumberByShoot();
            }
        }

        private void RoundNext()
        {
            if (!roundManager.Next())
            {
                Common.LogD("Game Over");
            }
            else
            {
                scoreBoard.RecordTimeStart();
            }
        }
    }
}
