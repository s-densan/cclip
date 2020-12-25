using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccliplog
{
    class JournalMeta
    {
        public string id { get; set; }
        public long creationDate { get; set; }
        public long modifiedDate { get; set; }
        public string version { get; set; }
        public string[] tags { get; set; }
        public bool starred { get; set; }
    }
    class JournalContents
    {
        public string text { get; set; }
        public int mood { get; set; }
        public string type { get; set; }
    }
    class Journal
    {
        public JournalMeta meta { get; set; }
        public JournalContents contents { get; set; }
        public Journal(string text)
        {
            var now = ToUnixTime(DateTime.Now);
            this.meta = new JournalMeta() {
                id = (now).ToString(),
                creationDate = now,
                modifiedDate = now,
                version = "1.0",
                tags = new string[]{ "ccliplog" },
                starred = false,
            };

            this.contents = new JournalContents()
            {
                text = text,
                mood = 0,
                type = "text/plain",
            };

        }
        public static long ToUnixTime(DateTime dt)
        {
            var dto = new DateTimeOffset(dt.Ticks, new TimeSpan(+09, 00, 00));
            return dto.ToUnixTimeMilliseconds();
        }
        public static DateTime FromUnixTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }

    }
}
