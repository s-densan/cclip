using cclip_lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                this.PostTextBox.Text = textData.First().Data.ToString();
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
