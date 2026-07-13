namespace MeteoLib.Impl.MeteoAPI
{
    public class Counter
    {
        readonly Dictionary<string, DateTime> _dict = new();
        readonly DateTime _initial;
        readonly TimeSpan _step;

        public Counter(DateTime initial, TimeSpan step)
        {
            _initial = initial;
            _step = step;
        }

        public DateTime this[string code]
        {
            get
            {
                return _dict.ContainsKey(code) ? _dict[code] : _initial;
            }
            set
            {
                if (_dict.ContainsKey(code))
                    _dict[code] = value;
                else
                    _dict.Add(code, value);
            }
        }

        public void Next(string code)
        {
            var next = this[code].Add(_step);
            this[code] = next;
        }
    }
}
