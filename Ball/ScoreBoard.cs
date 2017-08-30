using UnityEngine;

namespace BALL
{
    public class ScoreBoard : MonoBehaviour
    {
        public int ScoreBase;

        public int ScoreMin;

        public float ScoreReduceDelay;

        public float TimeScoreReduce;

        public int TimeScoreUnit;

        private int scoreTotal;

        public int ScoreTotal
        {
            get { return scoreTotal; }
            set { scoreTotal = value; }
        }

        private int scoreAdd;

        private float timeStart;

        public void RecordTimeStart()
        {
            timeStart = Time.time;
        }

        public void CalculateScore()
        {
            float timeStop = Time.time;
            scoreAdd = CalculateScore(ScoreBase, timeStop - timeStart);
            scoreTotal += scoreAdd;
            UpdateScoreEvent();
            Common.LogD("Update Score " + scoreAdd);
        }

        private int CalculateScore(int baseScore, float hitTime)
        {
            float time = hitTime - ScoreReduceDelay;
            time = time < 0 ? 0 : time;
            int timeScore = (int)(time / TimeScoreReduce * TimeScoreUnit);
            timeScore = timeScore < 0 ? 0 : timeScore;
            int score = baseScore - timeScore;
            score = score < ScoreMin ? ScoreMin : score;
            return score;
        }

        private void UpdateScoreEvent()
        {
            MCEvent scoreAddEvent = new MCEvent(MCEventType.UI_BALL_SHOW_SCORE_ADD);
            scoreAddEvent.IntValue = scoreAdd;
            MCEventCenter.instance.dispatchMCEvent(scoreAddEvent);

            MCEvent scoreEvent = new MCEvent(MCEventType.UI_BALL_UPDATE_SCORE);
            scoreEvent.IntValue = scoreTotal;
            MCEventCenter.instance.dispatchMCEvent(scoreEvent);
        }
    }
}
