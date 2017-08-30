using System.Collections.Generic;
using System.Linq;

namespace NumberShoot
{
    public class ScoreBoard
    {
        private bool scoreBestDirty;

        public bool ScoreBestDirty
        {
            get { return scoreBestDirty; }
            set { scoreBestDirty = value; }
        }

        private int scoreBest;

        public int ScoreBest
        {
            get { return scoreBest; }
            set 
            { 
                scoreBest = value;
                UpdateBestScoreEvent();
            }
        }

        private int scoreWeekBest;

        public int ScoreWeekBest
        {
            get { return scoreWeekBest; }
            set 
            { 
                scoreWeekBest = value;
                UpdateWeekBestScoreEvent();
            }
        }

        private int scoreTotal;

        public int ScoreTotal
        {
            get { return scoreTotal; }
            set 
            { 
                scoreTotal = value; 

                if (scoreTotal > scoreBest)
                {
                    ScoreBest = scoreTotal;
                    scoreBestDirty = true;
                }

                if (scoreTotal > scoreWeekBest)
                {
                    ScoreWeekBest = scoreTotal;
                    scoreBestDirty = true;
                }
            }
        }

        private int combo;

        public int Combo
        {
            get { return combo; }
            set
            { 
                combo = value;
                UpdateComboEvent();
            }
        }

        private float difficulty;
        private float difficultySum;

        public float DifficultySum
        {
            get { return difficultySum; }
            set { difficultySum = value; }
        }

        private List<int> saucerScore;

        public ScoreBoard()
        {
            saucerScore = new List<int>();
        }

        public void NewQuestion(float difficulty)
        {
            this.difficulty = difficulty;
            saucerScore.Clear();
        }

        public void ResetSaucerScore()
        {
            UpdateQuestionScoreEvent(0);
        }

        public void SaucerScore(int score)
        {
            saucerScore.Add(score);

            UpdateSaucerScoreEvent(score);

            Common.LogD("Update Saucer Score " + score);
        }

        public void BonusSaucerScore(int score)
        {
            ScoreTotal += score;

            UpdateSaucerScoreEvent(score);
            UpdateQuestionScoreEvent(score);

            Common.LogD("Update Bonus Saucer Score " + score);
        }

        public int CalculateQuestionScore()
        {
            //int score = (int)(saucerScore.Sum() * difficulty * (1.0f + 2.0f * (combo - 1) / 100.0f));
            int score = (int)(saucerScore.Sum() *  (1.0f + (combo - 1) / 10.0f));
            ScoreTotal += score;

            UpdateQuestionScoreEvent(score);

            difficultySum += difficulty;

            return score;
        }

        private void UpdateSaucerScoreEvent(int score)
        {
            MCEvent scoreAddEvent = new MCEvent(MCEventType.UI_NUMBER_SAUCER_SCORE);
            scoreAddEvent.IntValue = score;
            MCEventCenter.instance.dispatchMCEvent(scoreAddEvent);
        }

        private void UpdateQuestionScoreEvent(int score)
        {
            MCEvent scoreEvent = new MCEvent(MCEventType.UI_NUMBER_QUESTION_SCORE);
            scoreEvent.IntValue = score;
            MCEventCenter.instance.dispatchMCEvent(scoreEvent);
        }

        private void UpdateComboEvent()
        {
            MCEvent comboEvent = new MCEvent(MCEventType.UI_NUMBER_UPDATE_COMBO);
            comboEvent.IntValue = combo;
            MCEventCenter.instance.dispatchMCEvent(comboEvent);
        }

        private void UpdateBestScoreEvent()
        {
            MCEvent scoreEvent = new MCEvent(MCEventType.UI_NUMBER_BEST_SCORE);
            scoreEvent.IntValue = scoreBest;
            MCEventCenter.instance.dispatchMCEvent(scoreEvent);
        }

        private void UpdateWeekBestScoreEvent()
        {
            MCEvent scoreEvent = new MCEvent(MCEventType.UI_NUMBER_WEEK_BEST_SCORE);
            scoreEvent.IntValue = scoreWeekBest;
            MCEventCenter.instance.dispatchMCEvent(scoreEvent);
        }
    }
}
