using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LFMSync.Android.Utils.UI
{
    class ProgressAlert
    {
        public ProgressAlert(string title, string message, Context context)
        {
            builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetCancelable(false);
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

        public void Hide()
        {
            if (dialog != null)
            {
                dialog.Hide();
                dialog = null;
            }
        }
    }
}