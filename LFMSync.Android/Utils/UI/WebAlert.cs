using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;

namespace LFMSync.Android.Utils.UI
{
    class WebAlert
    {
        public WebAlert(string title, string url, Context context)
        {
            builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);
            var web = new WebView(context);
            web.Settings.JavaScriptEnabled = true;
            web.SetMinimumHeight(300);
            web.SetWebViewClient(new WebViewClient());
            web.LoadUrl(url);
            builder.SetView(web);
            builder.SetPositiveButton("OK", (e,a) => { });
        }

        AlertDialog.Builder builder;
        AlertDialog dialog;

        public void Show()
        {
            if (dialog == null)
            {
                dialog = builder.Create();
                dialog.Show();
            }
        }
    }
}