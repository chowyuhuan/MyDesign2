using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NumberShoot
{
    public class RoundManager : MonoBehaviour
    {
        public SaucerManager saucerManager;

        private QuestionGenerator questionGenerator;

        private Queue<CSV_c_number_question> questionList;

        private CSV_c_number_question currentQuestion;

        private List<string> currentQuestionElements;
        private List<string> cloneQuestionElements;

        private CSV_c_number_round_config currentRound;
        private List<float> roundDifficultyTable;
        private int currentRoundDifficulty;

        private int roundCounter;

        private float roundTimer;

        private int roundAnswerRight;

        private bool newGame;

        public int QuestionRightNumPerRound;

        public Action<float> OnNewQuestion;
        public Action OnNewRound;
        public Action OnGameOver;

        public int bonusTimeRoundNum;

        private int bonusRoundCounter;

        public float TimeAddSpeed;

        void Start()
        {
            questionGenerator = new QuestionGenerator();

            questionList = new Queue<CSV_c_number_question>();
            currentQuestionElements = new List<string>();
            cloneQuestionElements = new List<string>();

            roundDifficultyTable = new List<float>();

        }

        void Update()
        {
        }

        public void InitGuideRound()
        {
            roundCounter = 0;
            newGame = true;
        }

        public void InitNormalRound()
        {
            roundCounter = 1;
            bonusRoundCounter = 0;
            newGame = true;
        }

        public bool Next()
        {
            if (!RoundFinished())
            {
                NextQuestion();
                QuestionStart();
            }
            else
            {
                if (bonusRoundCounter++ >= bonusTimeRoundNum)
                {
                    bonusRoundCounter = 0;
                    EnterBonusTime();
                    BonusTimeEvent(true);
                }
                else
                {
                    BonusTimeEvent(false);
                    if (HaveNextRound())
                    {
                        Common.LogD("Start Round " + roundCounter);
                        StartCoroutine(NextRound());
                        OnNewRound();
                        newGame = false;
                    }
                    else
                    {
                        Common.LogD("All round finished");
                        return false;
                    }
                }
            }
            return true;
        }

        public void RoundEnd()
        {
            if (RoundFinished())
            {
                StopAllCoroutines();
            }
        }

        private void EnterBonusTime()
        {
            Common.LogD("Enter bonus time");
            saucerManager.InitSaucerBonus();
        }

        IEnumerator NextRound()
        {
            currentRound = CSV_c_number_round_config.GetData(roundCounter++);
            roundDifficultyTable.Clear();
            roundDifficultyTable.Add(currentRound.Difficulty1);
            roundDifficultyTable.Add(currentRound.Difficulty2);
            roundDifficultyTable.Add(currentRound.Difficulty3);
            roundDifficultyTable.Add(currentRound.Difficulty4);
            roundDifficultyTable.Add(currentRound.Difficulty5);
            currentRoundDifficulty = 0;
            roundAnswerRight = 0;

            NewRoundEvent();
            NextQuestion();

            if (!newGame)
            {
                TimeAddEvent();
                yield return new WaitForSeconds(1.5f);
            }

            QuestionIceEvent(true);
            float time = roundTimer + currentRound.Time;
            StartCoroutine(InitRoundTimer(time));
            while (roundTimer < time)
            {
                yield return new WaitForEndOfFrame();
            }
            QuestionIceEvent(false);

            QuestionStart();

            StartCoroutine("RoundTimeCountDown");

            Common.LogD("New round start " + roundCounter);
        }

        private bool HaveNextRound()
        {
            return roundCounter < CSV_c_number_round_config.DateCount;
        }

        public bool RoundFinished()
        {
            return newGame || RoundAnswerFinished();
        }

        private bool RoundAnswerFinished()
        {
            return roundAnswerRight >= QuestionRightNumPerRound;
        }

        IEnumerator RoundTimeCountDown()
        {
            while (roundTimer >= 0)
            {
                yield return new WaitForSeconds(1);

                RoundCountDownEvent(false);

                roundTimer--;
            }

            StopCoroutine("RoundTimeCountDown");

            OnGameOver();
        }

        IEnumerator InitRoundTimer(float time)
        {
            while (roundTimer < time)
            {
                yield return new WaitForSeconds(TimeAddSpeed);

                roundTimer++;

                RoundCountDownEvent(true);
            }

            StopCoroutine("InitRoundTimer");
        }

        private void NextQuestion()
        {
            if (currentRoundDifficulty < 0 || currentRoundDifficulty >= roundDifficultyTable.Count)
            {
                Common.LogD("NextQuestion Error " + roundDifficultyTable.Count);
            }
            float questionDifficulty = roundDifficultyTable[currentRoundDifficulty];
            InitQuestion(questionDifficulty, questionDifficulty);
            currentQuestion = questionList.Dequeue();
            currentQuestionElements.Clear();
            questionGenerator.ParseExpression(currentQuestion.Question, currentQuestionElements);
            saucerManager.InitSaucerRound(questionGenerator.ExhaustionExpressionAnswer(currentQuestionElements), currentQuestion.SkeetNum, currentQuestion.SkeetInterval, currentQuestion.FlyTime);
            CloneQuestion();
            OnNewQuestion(currentQuestion.Difficulty);
            NewQuestionEvent(true);
            Common.LogD("New question " + currentQuestion.Question);
        }

        private bool HaveNextQuestion()
        {
            return questionList.Count > 0;
        }

        public void QuestionStart()
        {
            saucerManager.StartEmitSaucer();
        }

        private void InitQuestion(float difficultyMin, float difficultyMax)
        {
            questionList.Clear();
            CSV_c_number_question question = questionGenerator.GeneratorOne(difficultyMin, difficultyMax);
            if (question != null)
            {
                questionList.Enqueue(question);
            }
        }

        private void CloneQuestion()
        {
            cloneQuestionElements.Clear();
            cloneQuestionElements.AddRange(currentQuestionElements);
        }

        public void ResetQuestion()
        {
            currentQuestionElements.Clear();
            currentQuestionElements.AddRange(cloneQuestionElements);
            NewQuestionEvent(false);
        }

        public void FillQuestion(int number)
        {
            for (int i = 0; i < currentQuestionElements.Count; i++ )
            {
                if (currentQuestionElements[i] == QuestionGenerator.ElementBlank)
                {
                    currentQuestionElements[i] = number.ToString();
                    NewQuestionEvent(false);
                    break;
                }
            }
        }

        public string FillQuestionByAnswer(int[] answer)
        {
            List<string> temp = new List<string>(currentQuestionElements);
            int count = 0;

            for (int i = 0; i < temp.Count; i++)
            {
                if (temp[i] == QuestionGenerator.ElementBlank)
                {
                    temp[i] = answer[count++].ToString();
                }
            }

            return string.Join("", temp.ToArray());
        }

        public bool IsQuestionFinished()
        {
            return !currentQuestionElements.Contains(QuestionGenerator.ElementBlank);
        }

        public bool IsQuestionAnsweredCorrect()
        { 
            return questionGenerator.CalculateExpression(currentQuestionElements);
        }

        private void NewRoundEvent()
        {
            MCEvent roundEvent = new MCEvent(MCEventType.UI_NUMBER_NEW_ROUNG);
            roundEvent.IntValue = roundCounter - 1;
            roundEvent.DictValue["QuestionNum"] = questionList.Count;
            MCEventCenter.instance.dispatchMCEvent(roundEvent);
        }

        private void TimeAddEvent()
        {
            MCEvent timeEvent = new MCEvent(MCEventType.UI_NUMBER_TIME_ADD);
            timeEvent.FloatValue = currentRound.Time;
            MCEventCenter.instance.dispatchMCEvent(timeEvent);
        }

        private void RoundCountDownEvent(bool isAddTime)
        {
            MCEvent roundEvent = new MCEvent(MCEventType.PLAYER_TIME_LEFT_UPDATE);
            roundEvent.IntValue = (int)roundTimer;
            roundEvent.BooleanValue = isAddTime;
            MCEventCenter.instance.dispatchMCEvent(roundEvent);
        }

        private void QuestionIceEvent(bool show)
        {
            MCEvent iceEvent = new MCEvent(MCEventType.UI_NUMBER_QUESTION_ICE);
            iceEvent.BooleanValue = show;
            MCEventCenter.instance.dispatchMCEvent(iceEvent);
        }

        private void NewQuestionEvent(bool isNew)
        {
            MCEvent questionEvent = new MCEvent(MCEventType.UI_NUMBER_UPDATE_QUESTION);
            questionEvent.IntValue = questionList.Count;
            questionEvent.FloatValue = currentQuestion.Difficulty;
            questionEvent.ListValue.AddRange(currentQuestionElements.ConvertAll<object>(x => (object)x));
            questionEvent.BooleanValue = isNew;
            MCEventCenter.instance.dispatchMCEvent(questionEvent);
        }

        public void WrongAnswerEvent()
        {
            MCEvent answerEvent = new MCEvent(MCEventType.UI_NUMBER_ANSWER);
            answerEvent.BooleanValue = false;
            MCEventCenter.instance.dispatchMCEvent(answerEvent);

            MCEvent sndEvent = new MCEvent(MCEventType.AUDIO_GAME_PLAY);
            sndEvent.StringValue = "Answer_Wrong";
            MCEventCenter.instance.dispatchMCEvent(sndEvent);
        }

        public void RightAnswerEvent()
        {
            roundAnswerRight++;
            currentRoundDifficulty++;

            MCEvent answerEvent = new MCEvent(MCEventType.UI_NUMBER_ANSWER);
            answerEvent.BooleanValue = true;
            MCEventCenter.instance.dispatchMCEvent(answerEvent);

            MCEvent sndEvent = new MCEvent(MCEventType.AUDIO_GAME_PLAY);
            sndEvent.StringValue = "Answer_Right";
            MCEventCenter.instance.dispatchMCEvent(sndEvent);
        }

        private void BonusTimeEvent(bool flag)
        {
            MCEvent bonusEvent = new MCEvent(MCEventType.UI_NUMBER_BONUS_TIME);
            bonusEvent.BooleanValue = flag;
            MCEventCenter.instance.dispatchMCEvent(bonusEvent);
        }
    }
}
