using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccliplog
{
    class Journey
    {
        public string id { get; set; } = "";
        public long date_modified { get; set; } = 0;
        public long date_journal { get; set; } = 0;
        public string timezone { get; set; } = "Asia/Tokyo";
        public string text { get; set; } = "";
        public string preview_text { get; set; } = "";
        public int mood { get; set; } = 0;
        public double? lat { get; set; } = double.MaxValue;
        public double? lon { get; set; } = double.MaxValue;
        public string address { get; set; } = "";
        public string label { get; set; } = "";
        public string folder { get; set; } = "";
        public int sentiment { get; set; } = 0;
        public bool favourite { get; set; } = false;
        public string music_title { get; set; } = "";
        public string music_artist { get; set; } = "";
        public string[] photos { get; set; } = Array.Empty<string>();
        public JourneyWeather weather { get; set; } = new JourneyWeather();
        public string[] tags { get; set; } = new string[] { };
        public string type { get; set; } = "markdown";

        /// <summary>
        /// ID生成
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string CreateID(long date)
        {
            var id = date.ToString() + "-" + GetRandomHexNumber(16).ToLower();
            return id;
        }
        private static string GetRandomHexNumber(int digits)
        {
            var buffer = new byte[digits / 2];
            var rand = new Random();
            rand.NextBytes(buffer);
            var result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
            {
                return result;
            }
            return result + rand.Next(16).ToString("X");
        }
        public string CreatePhotoID(int? no = null)
        {
            return CreatePhotoID(this.id, no);
        }
        public static string CreatePhotoID(string journeyID, int? no = null)
        {
            if (no.HasValue)
            {
                var photoID = $"{journeyID}-{no:00}{GetRandomHexNumber(14).ToLower()}";
                return photoID;
            }
            else
            {
                var photoID = $"{journeyID}-{GetRandomHexNumber(16).ToLower()}";
                return photoID;
            }
        }
    }
    class JourneyWeather
    {
        public int id { get; set; } = -1;
        public double? degree_c { get; set; } = double.MaxValue;
        public string description { get; set; } = "";
        public string icon { get; set; } = "";
        public string place { get; set; } = "";

    }
}
