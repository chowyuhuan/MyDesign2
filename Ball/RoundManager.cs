using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BALL
{
    public class RoundManager : MonoBehaviour
    {
        public BallManager ballManager;

        private Queue<int> targetNumberList;

        private int currentTargetNumber;

        public int CurrentTargetNumber
        {
            get { return currentTargetNumber; }
            set { currentTargetNumber = value; }
        }

        private int roundTime;

        public int RoundTime
        {
            get { return roundTime; }
            set { roundTime = value; }
        }

        private int roundCounter;

        private int dValueMin;
        private int dValueMax;

        void Start()
        {
            targetNumberList = new Queue<int>();

            roundTime = 0;
            roundCounter = 1;
        }

        void Update()
        {
        }

        public bool Next()
        {
            if (HaveNextTargetNumber())
            {
                NextTargetNumber();
            }
            else
            {
                Common.LogD("Round finished " + roundCounter);

                StopCoroutine("RoundTimeCountDown");

                if (HaveNextRound())
                {
                    NextRound();

                    StartCoroutine("RoundTimeCountDown");
                }
                else
                {
                    Common.LogD("All round finished");
                    return false;
                }
            }
            return true;
        }

        private void NextRound()
        {
            CSV_c_ball_round_config round = CSV_c_ball_round_config.GetData(roundCounter++);
            InitRoundTime(round.CountDown);
            InitTargetNumber(round.NumberCount);
            InitBallMaxCount(round.BallMaxCount);
            InitBallSpeed(round.SpeedMin, round.SpeedMax);
            dValueMin = round.DValueMin;
            dValueMax = round.DValueMax;

            NextTargetNumber();

            ballManager.RecycleAllBall();
            ballManager.GenerateBall();

            Common.LogD("New round start " + roundCounter);

            NewRoundEvent();
        }

        private bool HaveNextRound()
        {
            return roundCounter < CSV_c_ball_round_config.DateCount;
        }

        private void NewRoundEvent()
        {
            MCEvent roundEvent = new MCEvent(MCEventType.UI_BALL_NEW_ROUNG);
            roundEvent.IntValue = roundCounter - 1;
            MCEventCenter.instance.dispatchMCEvent(roundEvent);
        }

        private void NextTargetNumber()
        {
            currentTargetNumber = targetNumberList.Dequeue();
            InitBallNumberSample();
            NewTargetNumberEvent();
            Common.LogD("New target number " + currentTargetNumber);
        }

        private bool HaveNextTargetNumber()
        {
            return targetNumberList.Count > 0;
        }

        private void NewTargetNumberEvent()
        {
            MCEvent numberEvent = new MCEvent(MCEventType.UI_BALL_UPDATE_NUMBERS);
            numberEvent.ListValue.Add(currentTargetNumber);
            numberEvent.ListValue.InsertRange(1, targetNumberList.ToList<int>().ConvertAll<object>(x => (object)x));
            MCEventCenter.instance.dispatchMCEvent(numberEvent);
        }

        private void InitRoundTime(int countDown)
        {
            roundTime += countDown;
        }

        private void InitTargetNumber(int numberCount)
        {
            targetNumberList.Clear();
            for (int i = 0; i < numberCount; i++)
            {
                targetNumberList.Enqueue(Random.Range((int)BallNumberRange.Min, (int)BallNumberRange.Max));
            }
        }

        private void InitBallMaxCount(int ballMaxCount)
        {
            ballManager.BallMaxCount = ballMaxCount;
        }

        private void InitBallSpeed(float speedMin, float speedMax)
        {
            ballManager.BallSpeedMin = speedMin;
            ballManager.BallSpeedMax = speedMax;
        }

        private void InitBallNumberSample()
        {
            ballManager.BallNumberSample.Clear();

            for (int i = dValueMin; i <= dValueMax; i++)
            {
                ballManager.BallNumberSample.Add(BallCommon.RoundNumber(currentTargetNumber + i));
                ballManager.BallNumberSample.Add(BallCommon.RoundNumber(currentTargetNumber - i));
            }
        }

        IEnumerator RoundTimeCountDown()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                if (--roundTime == 0)
                {
                    StopCoroutine("RoundTimeCountDown");
                }

                MCEvent timeEvent = new MCEvent(MCEventType.PLAYER_TIME_LEFT_UPDATE);
                timeEvent.IntValue = roundTime;
                MCEventCenter.instance.dispatchMCEvent(timeEvent);
            }
        }
    }
}
