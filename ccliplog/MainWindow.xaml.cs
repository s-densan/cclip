using cclip_lib;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Path = System.IO.Path;

namespace ccliplog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ClipData[] ClipData;
        public string[] attachmentURLs = Array.Empty<string>();
        public MainWindow()
        {
            InitializeComponent();
            this.ClipData = Program.GetClipData();

            var textData = ClipData.Where(x => x.Format == "Text");
            var imageData = ClipData.Where(x => x.Format == "Bitmap");
            var fileData = ClipData.Where(x => x.Format == "FileDrop");
            var htmlData = ClipData.Where(x => x.Format == "HTML Format");

            // テキストボックス
            if (htmlData.Count() == 1)
            {
                var text = htmlData?.First().Data?.ToString()?.Trim() ?? "";
                this.PostTextBox.Text = HtmlToText(text);
            }
            else if (textData.Count() == 1)
            {
                var text = textData?.First().Data?.ToString()?.Trim() ?? "";
                var urlPattern = @"https?://[\w/:%#\$&\?\(\)~\.=\+\-]+";
                if (Regex.IsMatch(text, urlPattern))
                {
                    this.PostTextBox.Text = UrlToText(text);
                }
                else
                {
                    this.PostTextBox.Text = text;
                }
            }
            else if (fileData.Count() == 1)
            {
                this.PostTextBox.Text = string.Join("\n", fileData.First().Data as string[] ?? Array.Empty<string>());
            }
            else
            {
                this.PostTextBox.Text = "";
            }

            // 画像の添付
            if (ClipData.Select(x => x.Format).Contains("Bitmap"))
            {
                this.AttachFileLabel.Content = "画像の添付ファイルがあります。";
            }
            else
            {
                this.AttachFileLabel.Content = "";
            }
        }
        public static string HtmlToText(string htmlData)
        {
            var metaDict = new Dictionary<string, string>();
            foreach (var line in htmlData.Split("\r\n"))
            {
                if (line.Contains(":"))
                {
                    metaDict[line.Split(":")[0]] = string.Join(":", line.Split(":")[1..]);
                }
            }
            var html = new HtmlDocument();
            // HTML文字列を分析
            html.LoadHtml(htmlData);

            // 画像パスを収集
            var imagesXPath = "//img";
            var imageNodes = html.DocumentNode.SelectNodes(imagesXPath);
            var images = imageNodes?.Select(x => x.GetAttributeValue("src", ""));
            var startFragment = int.Parse(metaDict["StartFragment"]);
            var endFragment = int.Parse(metaDict["EndFragment"]);
            var urlText = metaDict.Keys.Contains("SourceURL") ? metaDict["SourceURL"].Split("?")[0] : null;
            var imageURLs = Array.Empty<Uri>();
            if (urlText != null)
            {
                imageURLs = (images ?? Array.Empty<string>()).Select(x => new Uri(new Uri(urlText), x)).ToArray();
            }

            // 画像をダウンロード
            foreach (var image in imageURLs.Select((v, i) => (v, i)))
            {
                using var wc = new WebClient();
                var savePath = @$"C:\Users\shimp\Desktop\images\{image.v.LocalPath.Split("/").Last()}";
                if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                {
                    //Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                }
                // wc.DownloadFile(image.v.AbsoluteUri, savePath);
            }

            var fragment = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(htmlData)[startFragment..endFragment]);
            var result = fragment + "\n\n" + urlText + "\n\n" + string.Join("\n", imageURLs.Select(x => $"- ![{x.LocalPath.Split("/").Last()}]({x.AbsoluteUri})"));
            return result;

        }

        public static string UrlToText(string url)
        {
            try
            {
                var str = new HttpClient().GetStringAsync(url).Result;
                var html = new HtmlDocument();
                html.LoadHtml(str);

                var titleXPath = "/html/head/title";
                var title = html.DocumentNode.SelectSingleNode(titleXPath).InnerText;
                var descriptionXPath = "/html/head/meta[@property=\"og:description\" or @property=\"description\"]";
                var description = html.DocumentNode.SelectSingleNode(descriptionXPath)?.GetAttributeValue("content", "") ?? "";
                var keyworesXPath = "/html/head/meta[@name=\"keywords\"]";
                var keywords = html.DocumentNode.SelectSingleNode(keyworesXPath)?.GetAttributeValue("content", "") ?? "";
                var result = @$"
## {title}

{description}

---

- url : {url}  
- keywords : {keywords}
";
                return result;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return "";
            }
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
        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            var j = new Journal(this.PostTextBox.Text);
            var json = System.Text.Json.JsonSerializer.Serialize(j);

            // SaveMemorize();
            SaveJourney();

            this.Close();
        }
        private void AddClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            this.ClipData = Program.GetClipData();
            var textData = ClipData.Where(x => x.Format == "Text");
            var imageData = ClipData.Where(x => x.Format == "Bitmap");
            var fileData = ClipData.Where(x => x.Format == "FileDrop");
            var htmlData = ClipData.Where(x => x.Format == "HTML Format");

            // テキストボックス
            if (htmlData.Count() == 1)
            {
                var text = htmlData?.First().Data?.ToString()?.Trim() ?? "";
                this.PostTextBox.Text += HtmlToText(text);
            }
            else if (textData.Count() == 1)
            {
                var text = textData?.First().Data?.ToString()?.Trim() ?? "";
                var urlPattern = @"https?://[\w/:%#\$&\?\(\)~\.=\+\-]+";
                if (Regex.IsMatch(text, urlPattern))
                {
                    this.PostTextBox.Text += UrlToText(text);
                }
                else
                {
                    this.PostTextBox.Text += text;
                }
            }
            else if (fileData.Count() == 1)
            {
                this.PostTextBox.Text += string.Join("\n", fileData.First().Data as string[] ?? Array.Empty<string>());
            }
            else
            {
                // do nothing
            }

            // 画像の添付
            if (ClipData.Select(x => x.Format).Contains("Bitmap"))
            {
                this.AttachFileLabel.Content = "画像の添付ファイルがあります。";
            }
            else
            {
                this.AttachFileLabel.Content = "";
            }
            
        }

        private void SaveJourney()
        {
            // ファイル作成
            var dirPath = Properties.Settings.Default.OutputDirPath;
            if (dirPath == "")
            {
                dirPath = Directory.GetCurrentDirectory();
            }
            else if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var now = ToUnixTime(DateTime.Now);

            // データ作成
            var photosData = this.ClipData.Where(x => x.Format == "Bitmap").Select(x => x.Data);
            byte[][] photos = { };
            if (photosData.Count() == 1)
            {
                photos = new byte[][] { (byte[]?)photosData.First() ?? Array.Empty<byte>() };
            }
            var jny = CreateJourney(now, this.PostTextBox.Text, photos);
            foreach(var photo in jny.photos.Zip(photos!))
            {
                var photoName = photo.First;
                var photoData = photo.Second;
                var photoPath = Path.Combine(dirPath, photoName) + ".png" ;
                File.WriteAllBytes(photoPath, photoData);
            }

            var filePath = Path.Combine(dirPath, jny.id) + ".json";
            var jnyJson = System.Text.Json.JsonSerializer.Serialize(jny);

            File.WriteAllText(filePath, jnyJson);

        }
        static private Journey CreateJourney(long now, string text, IEnumerable<byte[]>? photos)
        {
            var id = Journey.CreateID(now);
            var photoNames = new List<string>();

            if (photos?.Count() == 1)
            {
                var photoID = Journey.CreatePhotoID(id);
                photoNames.Add(photoID + ".png");
            }
            var jny = new Journey()
            {
                id = id,
                text = text,
                preview_text = text,
                date_journal = now,
                date_modified = now,
                photos = photoNames.ToArray(),
            };
            return jny;

        }
        private void SaveMemorize()
        {
            // ファイル作成
            var dirPath = Properties.Settings.Default.OutputDirPath;
            if (dirPath == "")
            {
                dirPath = Directory.GetCurrentDirectory();
            }
            else if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var now = ToUnixTime(DateTime.Now);
            var filePath = Path.Combine(dirPath, now.ToString()) + ".json";


            // データ作成
            var photosData = this.ClipData.Where(x => x.Format == "Bitmap").Select(x => x.Data);
            var photos = new string[] { };
            if (photosData.Count() == 1)
            {
                var photo = (byte[]?)photosData.First() ?? Array.Empty<byte>();
                var photoPath = Path.Combine(dirPath, now.ToString()) + ".png" ;
                File.WriteAllBytes(photoPath, photo);
                photos = new string[] { Path.GetFileName(photoPath)};
            } 
            var memo = new Memorize()
            {
                text = this.PostTextBox.Text,
                createdDate = now,
                photos = photos,

            };
            var memoJson = System.Text.Json.JsonSerializer.Serialize(memo);

            File.WriteAllText(filePath, memoJson);

        }
    }
}
