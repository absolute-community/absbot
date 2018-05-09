using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBot.Utils
{
    public static class UnixTimeExtension
    {
        
        static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime FromUnixTime(this long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        public static DateTime FromUnixTime(this string unixTime)
        {
            long time = 0;
            long.TryParse(unixTime, out time);
            return time.FromUnixTime();
        }

        public static long ToUnixTime(this DateTime date)
        {
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }
        
    }
}
