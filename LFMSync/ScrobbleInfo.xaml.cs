using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace LFMSync
{
    /// <summary>
    /// Interaction logic for ScrobbleInfo.xaml
    /// </summary>
    public partial class ScrobbleInfo
    {
        LastTrack track;
        public ScrobbleInfo(LastTrack t)
        {
            InitializeComponent();
            track = t;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = track;
            properties.ItemsSource = new Dictionary<string, object>()
            {
                {"Artist", track.ArtistName },
                { "Album", track.AlbumName },
                { "Track", track.Name },
                { "Duration", track.Duration },
                { "Time Played", track.TimePlayed },
                { "Listeners", track.ListenerCount },
            };
        }
    }
}
