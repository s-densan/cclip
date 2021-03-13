using cclip_lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccliplog
{
    class Memorize
    {
#pragma warning disable IDE1006 // 命名スタイル
        public string? address { get; set; } = "";
        public string[] audio {get;set;}= Array.Empty<string>();
        public long createdDate {get;set;}= 0;
        public double? latitude {get;set;}= null;
        public double? longitude {get;set;}= null;
        public int mood {get;set;}= 3;
        public int moodColor {get;set;}= -16121;
        public string moodIcon {get;set;}= "ic_emoticon_straight_2_white";
        public string moodName {get;set;}= "通常";
        public string[] photos {get;set;}= Array.Empty<string>();
        public string? placeName {get;set;}= null;
        public int starred {get;set;}= 0;
        public string[] tags {get;set;}= Array.Empty<string>();
        public double? temperature {get;set;}= null;
        public string text {get;set;}= "";
        public string? weatherCode {get;set;}= null;
        public string? weatherDescription {get;set;}= null;
#pragma warning restore IDE1006 // 命名スタイル
    }
}
