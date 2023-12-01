using System;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RegexCalc
{
    public partial class Form1 : Form
    {
        private readonly List<string> _operators = new List<string>() { "(", ")", "+", "-", "*", "/", "^" };

        public Form1() => InitializeComponent();

        private void RunBtn_Click(object sender, EventArgs e)
        {
            string expression = ExpressionTbx.Text.RemoveChar(' ');
            string formattedExpression = FormatNegative(expression).FormatDecimalSeparator();

            if (Regex.Matches(formattedExpression, @"\d*[-+*/^]\d+").Count > 0 &&
                formattedExpression.Count(c => c.Equals('(') || c.Equals(')')) % 2 == 0 &&
                formattedExpression.Count(c => char.IsLetter(c)) == 0)
            {
                AnswerLbl.Text = "Ответ: " + GetResult(formattedExpression);
            }
            else
                MessageBox.Show("Выражение записано не верно");
        }

        private string FormatNegative(string expression)
        {
            StringBuilder str = new StringBuilder(expression);
            bool prevIsOp = true;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '-')
                {
                    if (prevIsOp)
                    {
                        if (i > 0)
                        {
                            if (str[i - 1].Equals('('))
                            {
                                str[i - 2] = '-';
                                str[i - 1] = str[i + 1];
                                str = new StringBuilder(str.ToString().Remove(i, 3));
                            }
                            else
                            {
                                str.Insert(i, "(0");
                                int index = str.ToString().Skip(i + 3).TakeWhile(c => char.IsDigit(c)).Count();
                                str.Insert(i + 3 + index, ')');
                                i = i + 1 + index;
                                prevIsOp = false;
                                continue;
                            }
                        }
                        else
                        {
                            str.Insert(i, "(0");
                            int index = str.ToString().Skip(i + 3).TakeWhile(c => char.IsDigit(c)).Count();
                            str.Insert(i + 3 + index, ')');
                            i = i + 3 + index;
                            prevIsOp = false;
                            continue;
                        }
                    }
                    else
                        prevIsOp = true;
                }
                else if (char.IsDigit(str[i]))
                    prevIsOp = false;
                else if (_operators.Contains(str[i].ToString()))
                    prevIsOp = true;
            }

            return str.ToString();
        }

        private IEnumerable<string> Separate(string value)
        {
            int pos = 0;

            while (pos < value.Length)
            {
                string str = string.Empty + value[pos];
                if (!_operators.Contains(value[pos].ToString()))
                {
                    if (char.IsDigit(value[pos]))
                    {
                        for (int i = pos + 1; i < value.Length &&
                            (char.IsDigit(value[i]) ||
                            value[i].ToString() == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                            i++)
                        {
                            str += value[i];
                        }
                    }
                    else if (char.IsLetter(value[pos]))
                    {
                        for (int i = pos + 1; i < value.Length &&
                            (char.IsLetter(value[i]) || char.IsDigit(value[i]));
                            i++)
                        {
                            str += value[i];
                        }
                    }
                }
                yield return str;
                pos += str.Length;
            }
        }

        private string[] ToReversePolishNotation(string value)
        {
            List<string> outputSeparated = new List<string>();
            Stack<string> stack = new Stack<string>();

            try
            {
                foreach (string str in Separate(value))
                {
                    if (_operators.Contains(str[0].ToString()))
                    {
                        if (stack.Count > 0 && !str.Equals("("))
                        {
                            if (str.Equals(")"))
                            {
                                string s = stack.Pop();

                                while (!s.Equals("("))
                                {
                                    outputSeparated.Add(s);
                                    s = stack.Pop();
                                }
                            }
                            else if (str[0].Priority() > stack.Peek()[0].Priority())
                            {
                                stack.Push(str);
                            }
                            else
                            {
                                while (stack.Count > 0 && str[0].Priority() <= stack.Peek()[0].Priority())
                                {
                                    outputSeparated.Add(stack.Pop());
                                }

                                stack.Push(str);
                            }
                        }
                        else
                            stack.Push(str);
                    }
                    else
                        outputSeparated.Add(str);
                }
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Выражение записано не верно");
                return Array.Empty<string>();
            }

            if (stack.Count > 0)
            {
                foreach (string str in stack)
                    outputSeparated.Add(str);
            }

            return outputSeparated.ToArray();
        }

        public string GetResult(string value)
        {
            Stack<string> stack = new Stack<string>();
            Queue<string> queue = new Queue<string>(ToReversePolishNotation(value));

            string symbol;
            if (queue.Count > 0)
                symbol = queue.Dequeue();
            else
                return double.NaN.ToString();

            while (queue.Count >= 0)
            {
                if (!_operators.Contains(symbol))
                {
                    stack.Push(symbol.ToString());
                    symbol = queue.Dequeue();
                }
                else
                {
                    double summ = 0;

                    try
                    {
                        double a = Convert.ToDouble(stack.Pop());
                        double b = Convert.ToDouble(stack.Pop());

                        switch (symbol)
                        {
                            case "+":
                                summ = a + b;
                                break;
                            case "-":
                                summ = b - a;
                                break;
                            case "*":
                                summ = a * b;
                                break;
                            case "/":
                                summ = b / a;
                                break;
                            case "^":
                                summ = Math.Pow(b, a);
                                break;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show("Выражение записано не верно");
                        return double.NaN.ToString();
                    }

                    stack.Push(summ.ToString());

                    if (queue.Count > 0)
                        symbol = queue.Dequeue();
                    else
                        break;
                }
            }

            return stack.Pop();
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Поддерживаемые операции: + - * / ^ ( )");
        }
    }
}
