using cclip_lib;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Windows;
using Path = System.IO.Path;
using System.Drawing;
using System.Runtime.InteropServices;


namespace ccliplog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public JournalData journalData;
        public const ImageFormat DefaultImageFormat = ImageFormat.Png;
        public const string DefaultImageExt = ".png";
        public readonly string[] imageExtensions = new string[] { ".bmp", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".avif" };
        public readonly string[] audioExtensions = new string[] { ".mp3", ".wav", ".acc", ".m4a", ".ogg", ".wma" };
        public readonly string[] videoExtensions = new string[] { ".mp4", ".mkv", ".wmv", ".avi", ".mpg", ".mpeg" };
        public List<string> attachmentURLs = new();
        public List<string> attachmentFilePathes = new();
        public List<byte[]> attachmentFileData = new();
        public List<string> attachmentUrls = new();
        public MainWindow()
        {
            InitializeComponent();
            this.journalData = new JournalData();
            this.DataContext = this.journalData;

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

            if (html.DocumentNode.SelectNodes("/html/body/*").Count == 1 &&
               html.DocumentNode.SelectSingleNode("/html/body/*").Name.ToLower() == "img")
            {
                // 画像のみの場合
                var imageUrl = html.DocumentNode.SelectSingleNode("//img").GetAttributeValue("src", "");
                var alt = html.DocumentNode.SelectSingleNode("//img").GetAttributeValue("alt", "");
                var altText = alt != "" ? alt + " : " : "";
                var text = $"- {altText}{imageUrl}\n";

                return (text, Array.Empty<string>());
            }
            else
            {
                // 画像以外の場合
                // 画像パスを収集
                var imagesXPath = "//img";
                var imageNodes = html.DocumentNode.SelectNodes(imagesXPath);
                var images = imageNodes?.Select(x => (src: x.GetAttributeValue("src", ""), alt: x.GetAttributeValue("alt", "")));
                var startFragment = int.Parse(metaDict["StartFragment"]);
                var endFragment = int.Parse(metaDict["EndFragment"]);
                var urlText = metaDict.Keys.Contains("SourceURL") ? metaDict["SourceURL"].Split("?")[0] : null;
                var imageURLs = Array.Empty<(Uri uri, string alt)>();
                if (urlText != null)
                {
                    imageURLs = (images ?? Array.Empty<(string src, string alt)>()).Select(x => (uri: new Uri(new Uri(urlText), x.src), x.alt)).ToArray();
                }

                // 画像をダウンロード
                foreach (var image in imageURLs.Select((v, i) => (v, i)))
                {
                    using var wc = new WebClient();
                    var savePath = @$"C:\Users\shimp\Desktop\images\{image.v.uri.LocalPath.Split("/").Last()}";
                    if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                    {
                        //Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                    }
                    // wc.DownloadFile(image.v.AbsoluteUri, savePath);
                }


                var fragment = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(htmlData)[startFragment..endFragment]);
                var result = fragment + "\n\n" + urlText + "\n\n" + string.Join("\n", imageURLs.Select(x => $"- ![{x.alt}]({x.uri.AbsoluteUri})"));
                return (result, imageURLs.Select(x => x.uri.AbsoluteUri).ToArray());
            }


        }

        public static (string Text, string[] imageUrls) UrlToText(string url)
        {
            try
            {
                using var wc = new WebClient
                {
                    Encoding = Encoding.UTF8
                };
                wc.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                var bytes = wc.DownloadData(url);
                var enc = Utils.GetCode(bytes) ?? Encoding.UTF8;
                var str = enc.GetString(bytes);

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

                if (title != "")
                {
                    if (url != "")
                    {
                        result += $"## [{title}]({url})\n\n";
                    }
                    else
                    {
                        result += $"## {title}\n\n";
                    }

                }
                if (description != "")
                {
                    result += $"{description}\n\n";
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
                if (imageUrl != "")
                {
                    result += $"- {imageUrl}\n\n";
                }

                // 結果リターン
                if (imageUrl != "")
                {
                    return (result, new string[] { imageUrl });
                }
                else
                {
                    return (result, Array.Empty<string>());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return ("", Array.Empty<string>());
            }
        }

        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            // SaveMemorize();
            SaveJourney();

            this.Close();
        }
        private void AddClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            SetFormDataFromClipboarda(DefaultImageFormat);
        }
        private static MemoryStream? CreateBitmapFromDIB(byte[] bin)
        {
            var BITMAPFILEHEADER_SIZE = 14;
            var headerSize = BitConverter.ToInt32(bin, 0);

            // var pixelSize = bin.Length - headerSize;

            var fileSize = BITMAPFILEHEADER_SIZE + bin.Length;


            var bmpStm = new MemoryStream(fileSize);

            var writer = new BinaryWriter(bmpStm);
            // --- Bitmap File Header ---
            writer.Write(Encoding.ASCII.GetBytes("BM")); // Type

            writer.Write(fileSize); // Size
            writer.Write("00"); // reserved1,reserved2
            writer.Write(BITMAPFILEHEADER_SIZE + headerSize); // OffBits
                                                              // --- DIB ---
            writer.Write(bin);
            writer.Flush();

            bmpStm.Seek(0, SeekOrigin.Begin);
            return bmpStm;
        }
        private static MemoryStream? CreateBitmapFromDIB(MemoryStream dib) {

        　　var bin = dib.ToArray();
            return CreateBitmapFromDIB(bin);
        }
        private void SetFormDataFromClipboarda(ImageFormat imgFmt)
        {
            // var ClipData = Program.GetClipData(imgFmt);
            var ClipData = Program.GetClipData(imgFmt);
            var textData = ClipData.Where(x => x.Format == "Text");
            var imageData = ClipData.Where(x => x.Format == "Bitmap");
            var imageByteData = ClipData.Where(x => x.Format == "DeviceIndependentBitmap").Select(x=>CreateBitmapFromDIB((byte[])(x.Data!)));
            var fileData = ClipData.Where(x => x.Format == "FileDrop");
            var htmlData = ClipData.Where(x => x.Format == "HTML Format");
            this.TagsTextBox.Text = $"ccliplog, {Environment.MachineName}";

            // テキストボックス
            if (htmlData.Count() == 1)
            {
                var text = htmlData?.First().Data?.ToString()?.Trim() ?? "";
                //this.PostTextBox.Text += HtmlToText(text).Text;
                this.journalData.Text += HtmlToText(text).Text;
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
                        // this.PostTextBox.Text += resText;
                        this.journalData.Text += resText;
                    }
                    else
                    {
                        // this.PostTextBox.Text += text + "\n";
                        this.journalData.Text += text + "\n";
                    }
                    this.attachmentURLs.AddRange(resImageUrls);
                }
                else
                {
                    // this.PostTextBox.Text += text + "\n";
                    this.journalData.Text += text + "\n";
                }
            }
            else if (fileData.Count() == 1)
            {
                var files = fileData.First().Data as string[] ?? Array.Empty<string>();
                // this.PostTextBox.Text += string.Join("\n", files);
                this.journalData.Text += string.Join("\n", files);
                foreach (var file in files.OrderBy(x => x))
                {
                    if (this.imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        // 画像ファイルの場合
                        this.attachmentFilePathes.Add(file);
                    }
                }
            }
            else
            {
                // do nothing
            }

            // 画像の添付
            var photosData = imageData.Select(x => x.Data).Concat(imageByteData);
            byte[][] photos = Array.Empty<byte[]>();
            if (photosData.Any())
            {
                photos = new byte[][] { (byte[]?)photosData.First() ?? Array.Empty<byte>() };
            }
            photos.ToList().ForEach(x => this.attachmentFileData.Add(x));

            if (this.attachmentFileData.Count > 0)
            {
                this.AttachFileLabel.Content = $"画像の添付ファイルが{this.attachmentFileData.Count}件あります。";
            }
            else
            {
                this.AttachFileLabel.Content = "";
            }

        }


        private void SaveMDJournal()
        {
            // 変数定義
            // ファイル作成
            var dirPath = "";
            var photoNo = 1;
            var appPath = Assembly.GetEntryAssembly()?.Location;
            if (appPath == null)
            {
                return;
            }
            var appDir = Directory.GetParent(appPath)?.FullName ?? "";
            var config = new Config.Config(Path.Join(new string[] { appDir, "config.json" }));
            dirPath = config.data.outPath;

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var now = Utils.ToUnixTime(DateTime.Now);
            // URLからファイルダウンロード

            // var jny = CreateJourney(now, this.PostTextBox.Text);
            var jny = CreateJourney(now, this.journalData.Text);
            var mdJnl = CreateMDJournal(now, this.journalData.Text, "");

            jny.tags = TagsTextBox.Text.ToString()?.Split(",").Select(x => x.Trim()).ToArray() ?? Array.Empty<string>();

            // ダウンロードファイルの処理
            var downloadFilePathes = new List<string>();
            foreach (var url in this.attachmentURLs)
            {
                var client = new WebClient();
                if (!Uri.CheckSchemeName(url)) { continue; }
                var urlObj = new Uri(url);
                // var urlFileName = urlObj.Segments[^1];
                var urlFileName = jny.CreatePhotoID(photoNo) + System.IO.Path.GetExtension(urlObj.AbsolutePath.Split("/").Last());
                var savePath = Path.Join(dirPath, urlFileName);
                try
                {
                    client.DownloadFile(url, savePath);
                    downloadFilePathes.Add(urlFileName);
                    photoNo += 1;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"ダウンロードできませんでした。\n{e}");

                }
            }
            jny.photos = jny.photos.ToList().Concat(downloadFilePathes.ToArray()).ToArray();

            // クリップボードから追加した画像の作成
            // インデックスのみでループ
            foreach (var idx in (this.attachmentFileData.ToArray() ?? Array.Empty<byte[]>()))
            {
                var photoID = jny.CreatePhotoID(photoNo);
                jny.photos = jny.photos.Append(photoID + DefaultImageExt).ToArray();
                photoNo += 1;
            }
            foreach (var (photoName, photoData) in jny.photos.Zip(this.attachmentFileData!))
            {
                var photoPath = Path.Combine(dirPath, photoName);
                File.WriteAllBytes(photoPath, photoData);
            }
            // 添付ファイルの処理
            foreach (var attachmentFilePath in this.attachmentFilePathes)
            {
                var filename = jny.CreatePhotoID(photoNo) + Path.GetExtension(attachmentFilePath);
                var savePath = Path.Join(dirPath, filename);
                File.Copy(attachmentFilePath, savePath);
                jny.photos = jny.photos.ToList().Append(filename).ToArray();
                photoNo += 1;
            }



            var jsonOption = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            var filePath = Path.Combine(dirPath, jny.id) + ".json";
            var jnyJson = System.Text.Json.JsonSerializer.Serialize(jny, jsonOption);

            File.WriteAllText(filePath, jnyJson);

        }
        private void SaveJourney()
        {
            // 変数定義
            // ファイル作成
            var dirPath = "";
            var photoNo = 1;
            var appPath = Assembly.GetEntryAssembly()?.Location;
            if (appPath == null)
            {
                return;
            }
            var appDir = Directory.GetParent(appPath)?.FullName ?? "";
            var config = new Config.Config(Path.Join(new string[] { appDir, "config.json" }));
            dirPath = config.data.outPath;

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var now = Utils.ToUnixTime(DateTime.Now);
            // URLからファイルダウンロード

            // var jny = CreateJourney(now, this.PostTextBox.Text);
            var jny = CreateJourney(now, this.journalData.Text);

            // ダウンロードファイルの処理
            var downloadFilePathes = new List<string>();
            foreach (var url in this.attachmentURLs)
            {
                var client = new WebClient();
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) { continue; }
                var urlObj = new Uri(url);
                // var urlFileName = urlObj.Segments[^1];
                var urlFileName = jny.CreatePhotoID(photoNo) + System.IO.Path.GetExtension(urlObj.AbsolutePath.Split("/").Last());
                var savePath = Path.Join(dirPath, urlFileName);
                try
                {
                    client.DownloadFile(url, savePath);
                    downloadFilePathes.Add(urlFileName);
                    photoNo += 1;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"ダウンロードできませんでした。\n{e}");

                }
            }

            jny.photos = jny.photos.ToList().Concat(downloadFilePathes.ToArray()).ToArray();
            // インデックスのみでループ
            foreach (var idx in (this.attachmentFileData.ToArray() ?? Array.Empty<byte[]>()))
            {
                var photoID = jny.CreatePhotoID(photoNo);
                jny.photos = jny.photos.Append(photoID + DefaultImageExt).ToArray();
                photoNo += 1;
            }
            foreach (var (photoName, photoData) in jny.photos.Zip(this.attachmentFileData!))
            {
                var photoPath = Path.Combine(dirPath, photoName);
                File.WriteAllBytes(photoPath, photoData);
            }



            // 添付ファイルの処理
            foreach (var attachmentFilePath in this.attachmentFilePathes)
            {
                var filename = jny.CreatePhotoID(photoNo) + Path.GetExtension(attachmentFilePath);
                var savePath = Path.Join(dirPath, filename);
                File.Copy(attachmentFilePath, savePath);
                jny.photos = jny.photos.ToList().Append(filename).ToArray();
                photoNo += 1;
            }

            jny.tags = TagsTextBox.Text.ToString()?.Split(",").Select(x => x.Trim()).ToArray() ?? Array.Empty<string>();

            var jsonOption = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            var filePath = Path.Combine(dirPath, jny.id) + ".json";
            var jnyJson = System.Text.Json.JsonSerializer.Serialize(jny, jsonOption);

            File.WriteAllText(filePath, jnyJson);

        }
        static private MDJournal CreateMDJournal(long now, string text, string source)
        {
            var mdJnl = new MDJournal
            {
                meta = new MDJournalMeta
                {
                    createdAt = new DateTime(now),
                    updatedAt = new DateTime(now)
                },
                contents = text,
                source = "",
            };
            mdJnl.meta.keys["mdjournal"] = MDJournal.CreateID(mdJnl.meta.createdAt.Value, "CCL");
            mdJnl.meta.keys["journey"] = Journey.CreateID(now, 1);

            return mdJnl;

        }
        static private Journey CreateJourney(long now, string text)
        {
            var id = Journey.CreateID(now, 1);
            var jny = new Journey()
            {
                id = id,
                text = text,
                preview_text = text,
                date_journal = now,
                date_modified = now,
                photos = Array.Empty<string>(),
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
            var now = Utils.ToUnixTime(DateTime.Now);
            var filePath = Path.Combine(dirPath, now.ToString()) + ".json";


            // データ作成
            var photosData = this.attachmentFileData;
            var photos = Array.Empty<string>();
            if (photosData.Count == 1)
            {
                var photo = (byte[]?)photosData.First() ?? Array.Empty<byte>();
                var photoPath = Path.Combine(dirPath, now.ToString()) + DefaultImageExt;
                File.WriteAllBytes(photoPath, photo);
                photos = new string[] { Path.GetFileName(photoPath) };
            }
            var memo = new Memorize()
            {
                // text = this.PostTextBox.Text,
                text = this.journalData.Text,
                createdDate = now,
                photos = photos,

            };
            var jsonOption = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            var memoJson = System.Text.Json.JsonSerializer.Serialize(memo, jsonOption);

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
