using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib.Factories
{
    class DateCalculate
    {
        /// <summary>
        /// определеяет дату события, зная только день месяца, а также предшествующий день и месяц 
        /// отпределение происходи относительно текущей даты, 
        /// предполагая, что искомая дата была не далее, чем в двух месяцах от текущей
        /// </summary>
        /// <param name="ackday">день события</param>
        /// <param name="dayBefore">день, предшествующий событию</param>
        /// <param name="monBefore">день, предшествующий событию</param>
        /// <param name="utcnow">относительная текущая дата</param>
        /// <returns>дата события</returns>
        public static DateTime DecideProbableYearMonth(int ackday, DateTime utcnow)
        {

            var result = new DateTime(utcnow.Year, utcnow.Month, ackday);

            if (ackday < utcnow.Day)
                result = result.AddMonths(1);


            return DecideProbableYear(result.Day, result.Month, utcnow);
        }


        /// <summary>
        /// определеяет дату события, зная только день и месяц 
        /// </summary>
        /// <param name="ackday">день события</param>
        /// <param name="ackmon">месяц события</param>
        /// <param name="utcnow">относительная текущая дата</param>
        /// <returns>дата события</returns>
        public static DateTime DecideProbableYear(int ackday, int ackmon, DateTime utcnow)
        {
            var result = new DateTime(utcnow.Year, ackmon, ackday);
            var dif = utcnow - result;

            if (result.Month == 1 && dif > TimeSpan.FromDays(60))
                result = result.AddYears(1);
            else if (result.Month == 12 && dif > TimeSpan.FromDays(60))
                result = result.AddYears(-1);

            return result;
        }

        /// <summary>
        /// определеяет дату события, зная день, часы и минуту 
        /// </summary>
        /// <param name="ackday">день события</param>
        /// <param name="ackhour">часы события</param>
        /// <param name="ackminute">минуты события</param>
        /// <param name="utcnow">относительная текущая дата</param>
        /// <returns>дата события</returns>
        public static DateTime DecideProbableDate(int ackday, int ackhour, int ackminute, DateTime utcnow)
        {
            if (ackminute > 59)
            {
                if (ackhour < 24)
                    ackhour++;
                else
                {
                    ackhour = 0;
                    if (ackday < DateTime.DaysInMonth(utcnow.Year, utcnow.Month))
                        ackday++;
                    else
                        ackday = 1;
                    utcnow = utcnow.AddDays(1);
                }
                ackminute = 0;
            }

            if (ackhour > 23)
            {
                ackhour = 0;
                if (ackday < DateTime.DaysInMonth(utcnow.Year, utcnow.Month))
                    ackday++;
                else
                    ackday = 1;
                utcnow = utcnow.AddDays(1);

            }
            var date = DecideProbableYearMonth(ackday, utcnow);
            return new DateTime(date.Year, date.Month, date.Day, ackhour, ackminute, 0, DateTimeKind.Utc);
        }
    }
}
