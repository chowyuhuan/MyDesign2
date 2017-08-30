using System;
using System.Text.RegularExpressions;

namespace Calculator
{
    public class StringExpression
    {
        public static string ComputeStringExpression(string expression)
        {
            // The order is the Operator priority, from low to high
            string operators = "+-*/";

            foreach (char c in operators)
            {
                if (expression.Contains(c.ToString()))
                {
                    string[] strs = expression.Split(c.ToString().ToCharArray(), 2);

                    // Check negative
                    if (c == '-' && (strs[0] == "" || !Regex.IsMatch(strs[0], @"^[+-]?\d*[.]?\d*$")))
                    {
                        continue;
                    }

                    float value1 = Convert.ToSingle(ComputeStringExpression(strs[0]));
                    float value2 = Convert.ToSingle(ComputeStringExpression(strs[1]));

                    switch (c)
                    {
                        case '+':
                            return (value1 + value2).ToString();
                        case '-':
                            return (value1 - value2).ToString();
                        case '*':
                            return (value1 * value2).ToString();
                        case '/':
                            return value2 == 0 ? "0" : (value1 / value2).ToString(); 
                    }
                }
            }
            return expression;
        }

        public static string ComputeStringExpressionWithBracket(string expression)
        {
            if (expression.Contains("("))
            {
                int rightBracket = expression.IndexOf(')');
                string exp = expression.Substring(0, rightBracket);
                string back = expression.Substring(rightBracket + 1);
                int leftBracket = exp.LastIndexOf('(');
                string front = exp.Substring(0, leftBracket);
                exp = exp.Substring(leftBracket + 1);
                expression = front + ComputeStringExpression(exp) + back;
                return ComputeStringExpressionWithBracket(expression);
            }
            else
            {
                return ComputeStringExpression(expression);
            }
        }
    }
}
