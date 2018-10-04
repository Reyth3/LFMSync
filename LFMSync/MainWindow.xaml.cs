using IF.Lastfm.Core;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace LFMSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        LastfmClient client;

        ObservableCollection<LastTrack> ReythScrobbles { get; set; }
        ObservableCollection<LastTrack> GinaScrobbles { get; set; }
        public string SourceUser { get; set; }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SourceUser = "mrreynevan2";
            LastAuth auth = new LastAuth(Token.CLIENT, Token.SECRET);
            client = new LastfmClient(auth);

            ReythScrobbles = new ObservableCollection<LastTrack>();
            GinaScrobbles = new ObservableCollection<LastTrack>();
            leftListView.ItemsSource = ReythScrobbles;
            rightListView.ItemsSource = GinaScrobbles;
            await Authenticate();
            ReloadScrobbles();
        }

        private async Task Authenticate()
        {
            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "session.bak");
            if(File.Exists(path))
            {
                var json = JsonConvert.DeserializeObject<LastUserSession>(File.ReadAllText(path));
                client.Auth.LoadSession(json);
            }
            else
            {
                var li = new LogIn(client);
                li.ShowDialog();
            }
            if (client.Auth.UserSession == null)
                await this.ShowMessageAsync("Error!", "Last.FM user authorization failed.");
        }

        private async void ReloadScrobbles()
        {
            var progress = await this.ShowProgressAsync("Refreshing...", "Downloading data from Last.FM...");
            headers.DataContext = new
            {
                LeftName = SourceUser == "" || SourceUser == "" ? "N/A" : SourceUser,
                RightName = client.Auth != null ? client.Auth.UserSession.Username : "N/A",
            };

            if (SourceUser != null)
            {
                ReythScrobbles.Clear();
                var res = await client.User.GetRecentScrobbles(SourceUser, null, 1, 250);
                if (res.Success)
                {
                    foreach (var s in res)
                        ReythScrobbles.Add(s);
                }
            }
            if(client.Auth.UserSession != null)
            {
                var userRes = await client.User.GetRecentScrobbles(client.Auth.UserSession.Username, null, 1, 250);
                GinaScrobbles.Clear();
                if(userRes.Success)
                {
                    foreach (var s in userRes)
                        GinaScrobbles.Add(s);
                }
            }
            await progress.CloseAsync();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            ReloadScrobbles();
        }

        private async void SyncScrobblesClick(object sender, RoutedEventArgs e)
        {
            var progress = await this.ShowProgressAsync("Syncing...", "Scrobbling selected tracks...");
            var scrobbles = leftListView.SelectedItems.Cast<LastTrack>().ToList();
            var newScrobbles = scrobbles.Where(o => o.TimePlayed != null && !GinaScrobbles.Any(p => p.TimePlayed == o.TimePlayed));
            var list = newScrobbles.Select(o => new Scrobble(o.ArtistName, o.AlbumName, o.Name, o.TimePlayed.Value) { ChosenByUser = true, Duration = o.Duration }).ToList();
            var scrobbler = new Scrobbler(client.Auth);
            var res = await scrobbler.ScrobbleAsync(list);
            await progress.CloseAsync();
            await this.ShowMessageAsync("Syncing finished!", $"Synced {list.Count} scrobbles.");
            ReloadScrobbles();
        }

        private async void PickSource(object sender, RoutedEventArgs e)
        {
            SourceUser = await MahApps.Metro.Controls.Dialogs.DialogManager.ShowInputAsync(this, "Enter Username", "Enter the name of the profile from which you want to sync the scrobbles.");
            ReloadScrobbles();
        }

        private void ScrobbleInfoButtonClick(object sender, RoutedEventArgs e)
        {
            var scrobble = (sender as FrameworkElement).DataContext as LastTrack;
            var info = new ScrobbleInfo(scrobble);
            info.ShowDialog();
        }
    }
}
