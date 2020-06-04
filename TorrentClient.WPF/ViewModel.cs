using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TorrentClient.Client;

namespace TorrentClient.WPF
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string torrentLocation;
        public string TorrentLocation
        {
            get => torrentLocation;
            set
            {
                torrentLocation = value;
                OnPropertyChanged();
            }
        }

        private int port;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged();
            }
        }

        private ICommand browseCommand;
        public ICommand BrowseCommand
        {
            get
            {
                if (browseCommand == null)
                {
                    browseCommand = new RelayCommand(
                        param => Browse(),
                        param => CanBrowse()
                    );
                }
                return browseCommand;
            }
        }

        private ICommand startCommand;

        public ICommand StartCommand
        {
            get
            {
                if (startCommand == null)
                {
                    startCommand = new RelayCommand(
                        param => Start(),
                        param => CanStart()
                    );
                }
                return startCommand;
            }
        }

        private BitTorrentClient Client { get; set; }

        public ViewModel()
        {
            Port = 6969;
        }

        private void Browse()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".torrent",
                Filter = "Torrent Files|*.torrent"
            };

            bool? result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                string filename = dlg.FileName;
                TorrentLocation = filename;
            }
        }

        private bool CanBrowse()
        {
            return true;
        }

        private void Start()
        {
            Client = new BitTorrentClient(Port, TorrentLocation, @"C:\Users\pawpr\Desktop\Torrent\Output\");
            Client.Start();
            while (!Client.Torrent.IsCompleted)
            {

            }
            Client.Stop();
        }

        private bool CanStart()
        {
            try
            {
                if (TorrentLocation != null)
                    Path.GetFullPath(TorrentLocation);
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
