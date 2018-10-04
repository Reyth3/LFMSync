using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using LFMSync.Android.Utils.UI;
using Newtonsoft.Json;

namespace LFMSync.Android
{
    public class LastFMContext
    {
        Context Context;
        LastfmClient client;

        List<LastTrack> ReythScrobbles { get; set; }
        List<LastTrack> GinaScrobbles { get; set; }
        public string SourceUser { get; set; }

        public ScrobbleListAdapter ReythAdapter { get; set; }
        public ScrobbleListAdapter GinaAdapter { get; set; }

        public LastFMContext(Context context)
        {
            Context = context;
            //SourceUser = "mrreynevan2";
            LastAuth auth = new LastAuth(Token.CLIENT, Token.SECRET);
            client = new LastfmClient(auth);
            ReythScrobbles = new List<LastTrack>();
            GinaScrobbles = new List<LastTrack>();
        }

        public void SetAuth(string json)
        {
            var auth = JsonConvert.DeserializeObject<LastUserSession>(json);
            client.Auth.LoadSession(auth);
        }

        public bool Authenticate()
        {
            var prefs = Context.GetSharedPreferences("lfm", FileCreationMode.Private);
            if(prefs.Contains("session"))
            {
                var json = prefs.GetString("session", null);
                SetAuth(json);
                return true;
            }
            else
            {
                var intent = new Intent(Context, typeof(LogInActivity));
                Context.StartActivity(intent);
                return false;
            }
        }

        public async Task<bool> LogIn(string username, string password)
        {
            var res = await client.Auth.GetSessionTokenAsync(username, password);
            if (res.Success)
            {
                var prefs = Context.GetSharedPreferences("lfm", FileCreationMode.Private);
                var edit = prefs.Edit();
                var json = JsonConvert.SerializeObject(client.Auth.UserSession);
                edit.PutString("session", json);
                edit.Commit();
                return true;
            }
            else return false;
        }

        public async Task Reload(Context context = null)
        {
            if (context == null)
                context = Context;
            ProgressAlert alert = new ProgressAlert("Refreshing scrobbles", "Please wait...", context);
            alert.Show();
            if (SourceUser != null)
            {
                ReythScrobbles.Clear();
                var res = await client.User.GetRecentScrobbles(SourceUser, null, 1, 100);
                if (res.Success)
                    ReythScrobbles.AddRange(res);
            }
            if (client.Auth.UserSession != null)
            {
                var userRes = await client.User.GetRecentScrobbles(client.Auth.UserSession.Username, null, 1, 100);
                GinaScrobbles.Clear();
                if (userRes.Success)
                    GinaScrobbles.AddRange(userRes);
            }

            if (ReythAdapter == null)
                ReythAdapter = new ScrobbleListAdapter(ReythScrobbles.ToArray(), (e, a) =>
                {
                    var vh = a.View.Tag as ScrobbleListAdapterViewHolder;
                    vh.ChangeChecked();
                });
            else ReythAdapter.SetItems(ReythScrobbles);
            if (GinaAdapter == null)
                GinaAdapter = new ScrobbleListAdapter(GinaScrobbles.ToArray());
            else GinaAdapter.SetItems(GinaScrobbles);
            alert.Hide();
            if(string.IsNullOrEmpty(SourceUser))
            {
                var msg = new MessageAlert("Source User Not Set!", "Use the 'Set Source' option from the menu to pick the user whose scrobbles you want to sync.", context);
                msg.Show();
            }
        }

        public async Task<bool> Sync(LastTrack[] scrobbles)
        {
            var progress = new ProgressAlert("Syncing scrobbles", "Please wait...", Context);
            progress.Show();
            if (!client.Auth.Authenticated)
            {
                progress.Hide();
                return false;
            }
            var newScrobbles = scrobbles.Where(o => o.TimePlayed != null && !GinaScrobbles.Any(p => p.TimePlayed == o.TimePlayed));
            var list = newScrobbles.Select(o => new Scrobble(o.ArtistName, o.AlbumName, o.Name, o.TimePlayed.Value) { ChosenByUser = true, Duration = o.Duration }).ToList();
            var scrobbler = new MemoryScrobbler(client.Auth);
            var res = await scrobbler.ScrobbleAsync(list);
            progress.Hide();
            await Reload();
            Toast.MakeText(Context, $"Synced {list.Count} scrobbles!", ToastLength.Long).Show();
            return true;
        }
    }
}