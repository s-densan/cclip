using System;
using System.Collections.Generic;

namespace ccliplog
{
    class MDJournalAttachment
    {
        public string name = "";
        public string source = "";
    }
    class MDJournalMeta
    {
        public Dictionary<string, string> keys = new();
        public string address = "";
        public DateTime? createdAt = null;
        public DateTime? updatedAt = null;
        public List<MDJournalAttachment> photos = new();
        public List<MDJournalAttachment> videos = new();
        public List<string> tags = new();
        public string url = "";
        public double? latitude = null;
        public double? longitude = null;
        public int? mood = null;
        public string title = "";
    }
    class MDJournal
    {
        public MDJournalMeta meta = new MDJournalMeta();
        public string contents = "";
        public string source = "";

        public static string CreateID(DateTime datetime, string typeName, int index = 1)
        {
            var datetimeStr = datetime.ToString("yyyyMMddHHmmssfffffffff");
            var dateStr = datetimeStr[0..8];
            var timeStr = datetimeStr[8..];
            return $"{dateStr}-{timeStr}-{typeName}-{index:03}";
        }


    }
}
