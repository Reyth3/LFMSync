using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LFMSync.Android.Utils.UI;
using System.Linq;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace LFMSync.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        public static LastFMContext LastFM { get; set; }
        RecyclerView scrobblesView;
        FloatingActionButton syncButton;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            syncButton = FindViewById<FloatingActionButton>(Resource.Id.fab_sync);
            syncButton.Click += SyncScrobbles;
            if (LastFM == null)
                LastFM = new LastFMContext(this);
            LastFM.Authenticate();
            
            scrobblesView = FindViewById<RecyclerView>(Resource.Id.scrobblesView);
            await LastFM.Reload();
            scrobblesView.SetAdapter(LastFM.ReythAdapter);
            RecyclerView.LayoutManager lm = new LinearLayoutManager(this);
            scrobblesView.SetLayoutManager(lm);
        }

        private async void SyncScrobbles(object sender, System.EventArgs e)
        {
            if(string.IsNullOrEmpty(LastFM.SourceUser))
            {
                Toast.MakeText(this, "Pick the source user first.", ToastLength.Short).Show();
                return;
            }
            var scrobbles = LastFM.ReythAdapter.CheckedItems;
            if(scrobbles.Length == 0)
            {
                Toast.MakeText(this, "You need to select at least one scrobble.", ToastLength.Short).Show();
                return;
            }
            await LastFM.Sync(scrobbles.Select(o => o.Track).ToArray());
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actions, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_setsource:
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Last.fm username");
                    var input = new EditText(this);
                    alert.SetView(input);
                    alert.SetNegativeButton("Cancel", (o, ea) => { });
                    alert.SetPositiveButton("OK", async (o, ea) =>
                    {
                        LastFM.SourceUser = input.Text;
                        await LastFM.Reload();
                        scrobblesView.SetAdapter(LastFM.ReythAdapter);
                    });
                    alert.Show();
                    return true;
                case Resource.Id.action_reload:
                    RunOnUiThread(async () =>
                    {
                        await LastFM.Reload();
                        scrobblesView.SetAdapter(LastFM.ReythAdapter);
                    });
                    return true;
            }
            return false;
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_source:
                    scrobblesView.SetAdapter(LastFM.ReythAdapter);
                    return true;
                case Resource.Id.navigation_me:
                    scrobblesView.SetAdapter(LastFM.GinaAdapter);
                    return true;
                case Resource.Id.action_setsource:
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Last.fm username");
                    var input = new EditText(this);
                    alert.SetView(input);
                    alert.SetNegativeButton("Cancel", (o, ea) => { });
                    alert.SetPositiveButton("OK", async (o, ea) =>
                    {
                        LastFM.SourceUser = input.Text;
                        await LastFM.Reload();
                        scrobblesView.SetAdapter(LastFM.ReythAdapter);
                    });
                    alert.Show();
                    break;

            }
            return false;
        }
    }
}

