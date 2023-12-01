using System.Linq;
using System.Globalization;

namespace RegexCalc
{
    internal static class Extensions
    {
        public static string FormatDecimalSeparator(this string value)
        {
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Equals(","))
                return value.Replace(".", ",");
            else
                return value.Replace(",", ".");
        }

        public static string RemoveChar(this string value, char symbol) => new string(value.Where(c => c != symbol).ToArray());

        public static int Priority(this char symbol)
        {
            switch (symbol)
            {
                case '(':
                case ')':
                    return 0;
                case '+':
                case '-':
                    return 1;
                case '*':
                case '/':
                    return 2;
                case '^':
                    return 3;
                default:
                    return 4;
            }
        }
    }
}
