using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccliplog
{
    class Journey
    {
        public string id = "";
        public long date_modified = 0;
        public long date_journal = 0;
        public string timezone = "";
        public string text = "";
        public string preview_text = "";
        public int mood = 0;
        public double lat = 0;
        public double lon = 0;
        public string address = "";
        public string label = "";
        public string folder = "";
        public int sentiment = 0;
        public string favourite = "";
        public string music_title = "";
        public string music_artist = "";
        public string[] photos = { };
        public JourneyWeather weather = new JourneyWeather();
        public string[] tags = { };
        public string type = "";
    }
    class JourneyWeather
    {
        public int id = -1;
        public int? degree_c = null;
        public string? description = null;
        public string icon = "";
        public string place = "";

    }
}
