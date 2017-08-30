using UnityEngine;
using System.Collections.Generic;
using System;

namespace NumberShoot
{
    public class QuestionGenerator
    {
        public const string ElementBlank = "n";

        private List<string> Operations = new List<string> { "(", ")", "+", "-", "*", "/", "=" };

        private List<int> answeredQuestions;

        public QuestionGenerator()
        {
            answeredQuestions = new List<int>();

            //List<string> formule = new List<string> { "(", "3", "+", "1", ")", "/", "2", "+", "(", "4", "-", "2", ")", "=", "2" };
            //List<string> formule = new List<string> { "3", "+", "6", "/", "2", "-", "4", "*", "2", "=", "2" };
            //CalculateExpression(formule);

            //List<string> formule = new List<string> { "n", "/", "(", "n", "+", "n", ")", "=", "3" };
            //long start = DateTime.Now.Ticks;
            //ExhaustionExpressionAnswer(formule);
            //long end = DateTime.Now.Ticks;
            //Common.LogD("ExhaustionExpressionAnswer time :" + Convert.ToString((end - start) / 10000000.0));
        }

        public CSV_c_number_question GeneratorOne(float difficultyMin, float difficultyMax)
        {
            List<int> validQuestions = new List<int>();

            foreach(CSV_c_number_question question in CSV_c_number_question.AllData)
            {
                if (question.Difficulty >= difficultyMin && question.Difficulty <= difficultyMax)
                {
                    validQuestions.Add(question.ID);
                }
            }

            int questionId = 0;

            if (RandomQuestion(validQuestions, out questionId))
            {
                answeredQuestions.Add(questionId);
                return CSV_c_number_question.GetData(questionId);
            }
            return null;
        }

        private bool RandomQuestion(List<int> randomList, out int result)
        {
            if (randomList.Count > 1)
            {
                int random = UnityEngine.Random.Range(0, randomList.Count);
                int questionId = randomList[random];

                if (IsQuestionValid(questionId))
                {
                    Common.LogD("Generate question success : " + questionId + " " + CSV_c_number_question.GetData(questionId).Question);
                    result = questionId; 
                    return true;
                }
                else
                {
                    randomList.RemoveAt(random);
                    return RandomQuestion(randomList, out result);
                }
            }
            else
            {
                int questionId = randomList[0];
                Common.LogD("Generate question success : " + questionId + " " + CSV_c_number_question.GetData(questionId).Question);
                result = questionId;
                return true;
                //Common.LogD("Generate question failed");
                //result = -1;
                //return false;
            }
        }

        private bool IsQuestionValid(int questionId)
        {
            return !answeredQuestions.Contains(questionId);
        }

        public void ParseExpression(string expression, List<string> elements)
        {
            foreach (char c in expression)
            {
                if (Operations.Contains(c.ToString()))
                {
                    string[] strs = expression.Split(c.ToString().ToCharArray(), 2);
                    if (strs[0] != "")
                    {
                        elements.Add(strs[0]);
                    }
                    elements.Add(c.ToString());
                    ParseExpression(strs[1], elements);
                    return;
                }
            }
            elements.Add(expression);
        }

        public bool CalculateExpression(List<string> elements)
        {
            string expression = string.Join("", elements.ToArray());
            string[] strs = expression.Split('=');
            string result = Calculator.StringExpression.ComputeStringExpressionWithBracket(strs[0]);
            return strs[1].Equals(result);
        }

        public List<int[]> ExhaustionExpressionAnswer(List<string> elements)
        {
            List<string> elementsClone = new List<string>(elements);
            List<int[]> answers = new List<int[]>();

            // Search all variable index
            //int variableNum = elements.FindAll((x) => { return x == ElementBlank; }).Count;
            List<int> variableIndex = new List<int>();

            for (int i = 0; i < elementsClone.Count; i++)
            {
                if (elementsClone[i] == ElementBlank)
                {
                    variableIndex.Add(i);
                }
            }

            // Get exhaustion limit
            int limitCounter = (int)Mathf.Pow(10, variableIndex.Count);

            // Exhaustion algorithm
            for (int i = 0; i < limitCounter; i++)
            {
                int[] answer = new int[variableIndex.Count];

                for (int j = 0; j < variableIndex.Count; j++)
                {
                    int digit = (i / (int)(Mathf.Pow(10, j))) % 10;
                    elementsClone[variableIndex[j]] = Convert.ToString(digit);
                    answer[j] = digit;
                }

                if (CalculateExpression(elementsClone))
                {
                    answers.Add(answer);
                }
            }

            if (answers.Count == 0)
            {
                Common.LogD("ExhaustionExpressionAnswer no answers : " + string.Join("", elements.ToArray()));
            }

            return answers;
        }
    }
}
