using System;

namespace ccliplog
{
    class Journey
    {
#pragma warning disable IDE1006 // 命名スタイル
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
        public JourneyWeather weather { get; set; } = new();
        public string[] tags { get; set; } = Array.Empty<string>();
        public string type { get; set; } = "markdown";
#pragma warning restore IDE1006 // 命名スタイル

        /// <summary>
        /// ID生成
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string CreateID(long date, int num)
        {
            var id = date.ToString() + "-" + num.ToString("D16");
            return id;
        }
        public string CreatePhotoID(int no)
        {
            return CreatePhotoID(this.id, no);
        }
        public static string CreatePhotoID(string journeyID, int no)
        {
            var photoID = $"{journeyID}-{no:D16}";
            return photoID;
        }
    }
    class JourneyWeather
    {
#pragma warning disable IDE1006 // 命名スタイル
        public int id { get; set; } = -1;
        public double? degree_c { get; set; } = double.MaxValue;
        public string description { get; set; } = "";
        public string icon { get; set; } = "";
        public string place { get; set; } = "";
#pragma warning restore IDE1006 // 命名スタイル

    }
}
