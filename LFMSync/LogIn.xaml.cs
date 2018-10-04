using IF.Lastfm.Core.Api;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
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
using System.Windows.Shapes;

namespace LFMSync
{
    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    public partial class LogIn : MetroWindow
    {
        LastfmClient client;

        public LogIn(LastfmClient client)
        {
            InitializeComponent();
            this.client = client;
        }

        private async void LogInClick(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            b.IsEnabled = false;
            var res = await client.Auth.GetSessionTokenAsync(login.Text, password.Password);
            if (!res.Success)
                await this.ShowMessageAsync("Error", "Check your username and password and try again.");
            else
            {
                var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "session.bak");
                var json = JsonConvert.SerializeObject(client.Auth.UserSession);
                File.WriteAllText(path, json);
                this.Close();
            }
            b.IsEnabled = true;
        }
    }
}
