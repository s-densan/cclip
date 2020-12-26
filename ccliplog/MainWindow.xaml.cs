using cclip_lib;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.XPath;
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

            // テキストボックス
            if (textData.Count() == 1)
            {
                var text = textData.First().Data.ToString()?.Trim() ?? "";
                var urlPattern = @"https?://[\w/:%#\$&\?\(\)~\.=\+\-]+";
                if (Regex.IsMatch(text, urlPattern))
                {
                    var str = new HttpClient().GetStringAsync(text).Result;

                    var html = new HtmlDocument();
                    html.LoadHtml(str);

                    var title = html.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
                    var description =  html.DocumentNode.SelectSingleNode("/html/head/meta[@property=\"og:description\"]").GetAttributeValue("content", "");
                    this.PostTextBox.Text = @$"
## {title}

{description}

---

- url : {text}
";

                    /*
                    var request = WebRequest.Create(text);
                    var response = request.GetResponse();
                    var stream = response.GetResponseStream();
                    var sr = new StreamReader(stream);
                    var docNav = new XPathDocument(stream);
                    var nav = docNav.CreateNavigator();
                    var strExpression = @"//meta[@description]/text()";
                    var res = nav.Evaluate(strExpression);


                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(sr.ReadToEnd());
                    var nodeList = res;

                    this.PostTextBox.Text = nodeList?.ToString() ?? "";
                    */


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
                var photo = (byte[])photosData.First();
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
