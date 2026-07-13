using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib
{
    public class TriggerBuilder
    {
        private const int ZERO = 0;
        private const int WARM_LIMIT = 30;
        private const int COLD_LIMIT = -30;
        private const int WINDSPEED_LIMIT = 15;
        private const int VISIBLITY_LIMIT = 1000;

        public static Trigger Detect(MeteoRecord? prev, MeteoRecord cur)
        {
            var t = Trigger.CreateEmpty();

            if (prev != null && prev.Temperature >= ZERO && cur.Temperature < ZERO)
            {
                t.UnderZero = Level.HIGH;
            }

            if (cur.Temperature >= WARM_LIMIT)
            {
                if (prev != null && prev.Temperature >= WARM_LIMIT)
                    t.WarmLimit = Level.LOW;
                else t.WarmLimit = Level.HIGH;
            }

            if (cur.Temperature <= COLD_LIMIT)
            {
                if (prev != null && prev.Temperature <= COLD_LIMIT)
                    t.ColdLimit = Level.LOW;
                else t.ColdLimit = Level.HIGH;
            }

            if (cur.WindSpeed >= WINDSPEED_LIMIT)
            {
                if (prev != null && prev.WindSpeed >= WINDSPEED_LIMIT)
                    t.WindSpeedLimit = Level.LOW;
                else t.WindSpeedLimit = Level.HIGH;
            }

            if (cur.VisiblityValue <= VISIBLITY_LIMIT)
            {
                if (prev != null && prev.VisiblityValue <= VISIBLITY_LIMIT)
                    t.VisiblityLimit = Level.LOW;
                else t.VisiblityLimit = Level.HIGH;
            }

            foreach (var cond in cur.Conditions)
            {
                var level = prev?.Conditions is null ? Level.HIGH
                    : GetConditionLevel(cond, prev.Conditions);

                //if (prev?.Conditions is null)
                //{
                //    level = Level.HIGH;                    
                //}
                //else
                //{
                //    level = GetConditionLevel(cond, prev.Conditions);
                //}

                t.Conditions.Add(cond, level);

                //    t.Conditions.Add(cond, GetConditionLevel(cond, prev.Conditions));
                //else t.Conditions.Add(cond, Level.HIGH);
            }


            return t;

        }


        private static Level GetConditionLevel(string cond, List<string> prevConds)
        {
            ArgumentNullException.ThrowIfNull(cond, nameof(cond));
            ArgumentNullException.ThrowIfNull(prevConds, nameof(prevConds));
            //if (prevConds == null) return Level.HIGH;

            if (prevConds.Contains(cond)) return Level.LOW;//явление уже было
            if (cond.StartsWith("-") && prevConds.Contains(cond.Replace("-", ""))) return Level.LOW; //было явление большей интенсивности
            if (cond.StartsWith("-") && prevConds.Contains(cond.Replace("-", "+"))) return Level.LOW;
            if (prevConds.Contains("+" + cond)) return Level.LOW;
            return Level.HIGH; //новое явление
        }

    }
}