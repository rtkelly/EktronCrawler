using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EktronCrawler
{
    public static class ExtensionMethods
    {

        public static DateTime CustomAddTime(this DateTime date, string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr))
                return date;

            var time = DateTime.Parse("01/01/01 " + timeStr);
            
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }
    }
}
