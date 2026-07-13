using System.Text.RegularExpressions;

namespace MeteoLib.Impl.Delivery
{

    //https://www.markdownguide.org/basic-syntax/
    public class TelegramFormatter : IFormatter
    {
        public string BaseMeteoUrl => "meteo.ar-management.ru";

        public object BaseProtocol => "https";

        public string Bold(string text)
        {

            return $"*{Escape(text)}*";
        }

        private static string Escape(string text)
        {
            return text
                    .Escape("!.+-()".ToArray());
            //.Escape('.')
            //.Escape('+')
            //.Escape('(')
            //.Escape(')');

        }

        public string BreakLine()
        {
            return "\n";
        }

        public string ColorText(string text, Color color)
        {
            return Escape(text);
            //return $"<p style=\"color:{color}\">{text}</p>";
        }

        public string Link(string url, string label)
        {
            return $"[{label}]({url})";
        }

        public string RegularText(string text)
        {
            return Escape(text);
        }
    }


    internal static class Extestions
    {
        public static string Escape(this string value, char[] symbols)
        {
            var s = value;

            foreach (char c in symbols)
            {
                s = EscapeSingle(s, c);
            }


            return s;
            //var s = EscapeRegexpSymbol(symbol);

            //var pattern = $"(?<!\\\\){s}";
            //var replace = $"\\{symbol}";

            //return new Regex(pattern).Replace(value, replace);
        }


        public static string EscapeSingle(string value, char symbol)
        {
            var s = EscapeRegexpSymbol(symbol);

            var pattern = $"(?<!\\\\){s}";
            var replace = $"\\{symbol}";

            return new Regex(pattern).Replace(value, replace);
        }

        private static string EscapeRegexpSymbol(char symbol)
        {
            return symbol switch
            {
                '.' => "\\.",
                ')' => "\\)",
                '(' => "\\(",
                '+' => "\\+",
                '-' => "\\-",
                _ => symbol.ToString()
            };

            //var re = symbol == '.' ? $"\\." : symbol.ToString();
        }
    }
}