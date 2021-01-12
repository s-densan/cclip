using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ccliplog
{
    public class JournalData : INotifyPropertyChanged
    {
        public List<string> AttachmentURLs { get; set; } = new List<string>();
        public List<string> AttachmentFilePathes { get; set; } = new List<string>();
        public List<byte[]> AttachmentFileData { get; set; } = new List<byte[]>();
        public List<string> AttachmentUrls { get; set; } = new List<string>();
        private string _Text = "";
        private string _Tags = "";
        /// <summary>
        /// View Modelのルールとして実装しておくイベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private ICommand? _clearButtonCommand;
        private ICommand? _changeTextCommand;

        public ICommand ClearButton
        {
            get
            {
                if (_clearButtonCommand == null) {
                    _clearButtonCommand = new ClearButtonCommand(this);
                }
                return _clearButtonCommand;
            }
        }
        public ICommand ChangeTextCommand
        {
            get
            {
                if (_changeTextCommand == null) {
                    _changeTextCommand = new ChangeTextCommand(this);
                }
                return _changeTextCommand;
            }
        }

        public string Tags
        {
            get
            {
                // カウンタークラスが保持するカウント値を返す
                return this._Tags;
            }
            set
            {
                // カウンタークラスが保持するカウント値を設定する
                this._Tags = value;

                // 中身が変更されたことを View Modelに通知するためのイベントハンドラ呼び出し
                // 引数として、プロパティ名を文字列として渡すことでVIEWからバインドされる
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
            }
        }
        public string Text
        {
            get
            {
                // カウンタークラスが保持するカウント値を返す
                return this._Text;
            }
            set
            {
                // カウンタークラスが保持するカウント値を設定する
                this._Text = value;

                // 中身が変更されたことを View Modelに通知するためのイベントハンドラ呼び出し
                // 引数として、プロパティ名を文字列として渡すことでVIEWからバインドされる
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
    }
    class ClearButtonCommand : ICommand
    {
        private readonly JournalData _viewModel;

        public ClearButtonCommand(JournalData viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _viewModel.Text = "kiero!";
        }
    }

    class ChangeTextCommand : System.Windows.Input.ICommand
    {
        private readonly JournalData _viewModel;

        public ChangeTextCommand(JournalData viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _viewModel.Text = "kiero!";
        }
    }

}
