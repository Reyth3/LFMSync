using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using LFMSync.Android.Utils.UI;

namespace LFMSync.Android
{
    [Activity(Label = "Last.fm Authentication")]
    public class LogInActivity : AppCompatActivity
    {
        EditText username;
        EditText password;
        Button loginButton;
        TextView linkPrivacy;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_login);
            username = FindViewById<EditText>(Resource.Id.login_username);
            password = FindViewById<EditText>(Resource.Id.login_password);
            loginButton = FindViewById<Button>(Resource.Id.login_loginbutton);
            linkPrivacy = FindViewById<TextView>(Resource.Id.link_privacy);

            loginButton.Click += async (o, ea) =>
            {
                loginButton.Enabled = false;
                var login = await MainActivity.LastFM.LogIn(username.Text, password.Text);
                if (!login)
                {
                    var t = Toast.MakeText(this, "Couldn't log in. Check your username and password.", ToastLength.Short);
                    t.Show();
                }
                else
                {
                    await MainActivity.LastFM.Reload(this);
                    this.Finish();
                }
                loginButton.Enabled = true;
            };

            linkPrivacy.Click += (o, e) =>
            {
                var web = new WebAlert("Privacy Policy", "https://reyth3.github.io/apps/lfmsync/privacy.html", this);
                web.Show();
            };
        }
    }
}