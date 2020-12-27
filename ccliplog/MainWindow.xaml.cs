using cclip_lib;
using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            var html = new HtmlDocument();
            // HTML文字列を分析
            html.LoadHtml(htmlData);

            // 画像パスを収集
            var imagesXPath = "//img";
            var imageNodes = html.DocumentNode.SelectNodes(imagesXPath);
            var images = imageNodes?.Select(x => x.GetAttributeValue("src", ""));
            var urlText = string.Join(":", htmlData.Split("\r\n")[5].Split(":")[1..]).Split("?")[0];
            var imageURLs = (images ?? Array.Empty<string>()).Select(x => new Uri(new Uri(urlText), x));

            // 画像をダウンロード
            foreach (var image in imageURLs.Select((v, i) => (v, i)))
            {
                using var wc = new WebClient();
                var savePath = @$"C:\Users\shimp\Desktop\images\{image.v.LocalPath.Split(@"/").Last()}";
                if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                }
                wc.DownloadFile(image.v.AbsoluteUri, savePath);
            }

            return string.Join("\n", imageURLs); 

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
                var description = html.DocumentNode.SelectSingleNode( descriptionXPath)?.GetAttributeValue("content", "") ?? "";
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
            MessageBox.Show(json);

            SaveMemorize();

            this.Close();
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
