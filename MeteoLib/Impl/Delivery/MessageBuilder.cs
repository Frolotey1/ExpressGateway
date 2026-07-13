using System.Text;
using static MeteoLib.SourceText;

namespace MeteoLib.Impl.Delivery
{
    public class MessageBuilder
    {
        private readonly IFormatter _formatter;
        private readonly Trigger _trigger;
        private readonly string _asset;
        private SourceText _sourceText = new(Language.RUS);

        private string _ATTENTION_HEADER;
        private string _CONTINUES_HEADER;
        private string _OCCURENCES_HIGH;
        private string _OCCURENCES_LOW;
        private string _METEO_LINK;
        private string _BREAK;

        public MessageBuilder(string asset, Trigger trigger, IFormatter formatter)
        {
            _formatter = formatter;
            _trigger = trigger;
            _asset = asset;
        }


        public string BuildMessage()
        {

            // build message parts
            BoldRedHeader("Внимание!", ref _ATTENTION_HEADER);
            BreakLine(ref _BREAK);
            Occurences(Level.HIGH, ref _OCCURENCES_HIGH);
            Occurences(Level.LOW, ref _OCCURENCES_LOW);

            BoldHeader("Продолжается:", ref _CONTINUES_HEADER);

            MeteoLink(ref _METEO_LINK);



            //build final message

            var s = new StringBuilder();


            if (!string.IsNullOrEmpty(_OCCURENCES_HIGH))
            {
                s.Append(_ATTENTION_HEADER);
                s.Append(_BREAK);
                s.Append(_OCCURENCES_HIGH);
            }

            if (!string.IsNullOrEmpty(_OCCURENCES_LOW))
            {
                s.Append(_CONTINUES_HEADER);
                s.Append(_BREAK);
                s.Append(_OCCURENCES_LOW);
            }

            s.Append(_METEO_LINK);

            var result = s.ToString();




            return result;
        }

        private void MeteoLink(ref string output)
        {
            var baseUrl = _formatter.BaseMeteoUrl;
            var baseProtocol = _formatter.BaseProtocol;

            var label = _formatter.ColorText(baseUrl, Color.GRAY);
            output = _formatter.Link($"{baseProtocol}://{baseUrl}/?{_asset.ToLower()}", label);
        }

        private void Occurences(Level level, ref string output)
        {
            Occurence(level, _trigger.UnderZero, "Переход температуры через ноль к отрицательной", ref output);
            Occurence(level, _trigger.WarmLimit, "Температура воздуха выше +30°", ref output);
            Occurence(level, _trigger.ColdLimit, "Температура воздуха ниже -30°", ref output);
            Occurence(level, _trigger.WindSpeedLimit, "Порыв ветра свыше 15 м/с", ref output);
            Occurence(level, _trigger.VisiblityLimit, "Ухудшение видимости менее 1000 м", ref output);

            Conditions(level, ref output);

        }

        private void Conditions(Level level, ref string output)
        {
            var conditions = from c in _trigger.Conditions
                             where c.Value == level
                             select _sourceText.GetConditionString(c.Key);

            if (!conditions.Any()) return;

            var str = string.Join(";", conditions);

            output += _formatter.RegularText(str);
            BreakLine(ref output);
        }

        private void Occurence(Level expectedLevel, Level actualLevel, string messageText, ref string output)
        {
            if (expectedLevel != actualLevel) return;

            output += _formatter.RegularText(messageText);
            BreakLine(ref output);


        }

        private void BreakLine(ref string output)
        {
            output += _formatter.BreakLine();
        }

        private void BoldRedHeader(string text, ref string output)
        {
            output = _formatter.ColorText(text, Color.RED);
            output = _formatter.Bold(output);
        }

        private void BoldHeader(string text, ref string output)
        {
            output = _formatter.Bold(text);
        }
    }
}