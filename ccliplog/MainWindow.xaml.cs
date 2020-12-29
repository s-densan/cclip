using cclip_lib;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
        public const ImageFormat DefaultImageFormat = ImageFormat.Bmp;
        public const string DefaultImageExt = ".bmp";
        public List<string> attachmentURLs = new List<string>();
        public List<string> attachmentFilePathes = new List<string>();
        public List<byte[]> attachmentFileData = new List<byte[]>();
        public List<string> attachmentUrls = new List<string>();
        public MainWindow()
        {
            InitializeComponent();

            SetFormDataFromClipboarda(DefaultImageFormat);
        }
        public static (string Text, string[] ImageUrls) HtmlToText(string htmlData)
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
            return (result, imageURLs.Select(x => x.AbsoluteUri).ToArray());

        }

        public static (string Text, string[] imageUrls) UrlToText(string url)
        {
            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    wc.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                    var str = wc.DownloadString(url);

                    // var httpClient = new HttpClient();
                    // var task = httpClient.GetStringAsync(url);
                    // var str = task.Result;
                    var html = new HtmlDocument();
                    html.LoadHtml(str);

                    var titleXPath = "(/html/head|/html/body)/title";
                    var title = html.DocumentNode.SelectSingleNode(titleXPath)?.InnerText.Replace("\r", "").Replace("\n", "");
                    var descriptionXPath = "(/html/head|/html/body)/meta[@property=\"og:description\" or @property=\"description\"]";
                    var description = html.DocumentNode.SelectSingleNode(descriptionXPath)?.GetAttributeValue("content", "") ?? "";
                    var keyworesXPath = "(/html/head|/html/body)/meta[@name=\"keywords\"]";
                    var keywords = html.DocumentNode.SelectSingleNode(keyworesXPath)?.GetAttributeValue("content", "") ?? "";
                    var imageUrlXPath = "(/html/head|/html/body)/meta[@property=\"og:image\"]";
                    var imageUrl = html.DocumentNode.SelectSingleNode(imageUrlXPath)?.GetAttributeValue("content", "") ?? "";
                    var result = "";
                    result += $"## {title}\n\n";
                    if (description != "")
                    {
                        result += $"{description}\n\n";
                    }
                    if (imageUrl != "")
                    {
                        result += $"![]({imageUrl})\n\n";
                    }
                    result += $"---\n\n";
                    if (url != "")
                    {
                        result += $"- url : {url}\n";
                    }
                    if (keywords != "")
                    {
                        result += $"- keywords : {keywords}\n";
                    }
                    return (result, new string[] { imageUrl });
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return ("", Array.Empty<string>());
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
            SetFormDataFromClipboarda(DefaultImageFormat);
        }
        private void SetFormDataFromClipboarda(ImageFormat imgFmt)
        {
            var ClipData = Program.GetClipData(imgFmt);
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
                    var (resText, resImageUrls) = UrlToText(text);
                    if (resText != "")
                    {
                        this.PostTextBox.Text += resText;
                    }
                    else
                    {
                        this.PostTextBox.Text += text + "\n";
                    }
                    this.attachmentURLs.AddRange(resImageUrls);
                }
                else
                {
                    this.PostTextBox.Text += text + "\n";
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
            var photosData = ClipData.Where(x => x.Format == "Bitmap").Select(x => x.Data);
            byte[][] photos = Array.Empty<byte[]>();
            if (photosData.Count() == 1)
            {
                photos = new byte[][] { (byte[]?)photosData.First() ?? Array.Empty<byte>() };
            }
            photos.ToList().ForEach(x => this.attachmentFileData.Add(x));

            if (this.attachmentFileData.Count() > 0)
            {
                this.AttachFileLabel.Content = $"画像の添付ファイルが{this.attachmentFileData.Count()}件あります。";
            }
            else
            {
                this.AttachFileLabel.Content = "";
            }

        }


        private void SaveJourney()
        {
            // 変数定義
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
            // URLからファイルダウンロード
            var downloadFilePathes = new List<string>();
            foreach(var url in this.attachmentURLs)
            {
                var client = new WebClient();
                var urlObj = new Uri(url);
                // var urlFileName = urlObj.Segments[^1];
                var urlFileName = Journey.CreatePhotoID();
                var savePath = Path.Join(dirPath, urlFileName);
                client.DownloadFile(url, savePath);
                downloadFilePathes.Add(savePath);
            }

            var jny = CreateJourney(now, this.PostTextBox.Text, this.attachmentFileData, downloadFilePathes);
            foreach(var (photoName, photoData) in jny.photos.Zip(this.attachmentFileData!))
            {
                var photoPath = Path.Combine(dirPath, photoName);
                File.WriteAllBytes(photoPath, photoData);
            }

            var filePath = Path.Combine(dirPath, jny.id) + ".json";
            var jnyJson = System.Text.Json.JsonSerializer.Serialize(jny);

            File.WriteAllText(filePath, jnyJson);

        }
        static private Journey CreateJourney(long now, string text, IEnumerable<byte[]>? photos, IEnumerable<string>? pathes)
        {
            var id = Journey.CreateID(now);
            var photoNames = new List<string>();

            // インデックスのみでループ
            foreach (var idx in (photos ?? Array.Empty<byte[]>()).Select((v,i) => i))
            {
                var photoID = Journey.CreatePhotoID(id, idx + 1);
                photoNames.Add(photoID + DefaultImageExt);
            }
            // ファイルパスループ
            foreach (var path in pathes ?? Array.Empty<string>())
            {
                photoNames.Add(Path.GetFileName(path));
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
            var photosData = this.attachmentFileData;
            var photos = new string[] { };
            if (photosData.Count == 1)
            {
                var photo = (byte[]?)photosData.First() ?? Array.Empty<byte>();
                var photoPath = Path.Combine(dirPath, now.ToString()) + DefaultImageExt;
                File.WriteAllBytes(photoPath, photo);
                photos = new string[] { Path.GetFileName(photoPath) };
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

        private void OpenConfigFileButton_Click(object sender, RoutedEventArgs e)
        {

            Properties.Settings.Default.OutputDirPath = Properties.Settings.Default.OutputDirPath;
            Properties.Settings.Default.Save();
            var configPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

            var dirPath = Path.GetDirectoryName(configPath);
            var cmd = $"explorer \"{dirPath}\"";
            Process.Start(cmd);

        }
    }
}
