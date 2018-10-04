using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using IF.Lastfm.Core.Objects;
using Square.Picasso;
using System.Linq;
using System.Collections.Generic;

namespace LFMSync.Android
{
    public class ScrobbleData
    {
        public ScrobbleData(LastTrack track)
        {
            Track = track;
        }

        public LastTrack Track { get; set; }
        public bool IsChecked { get; set; }
    }

    public class ScrobbleListAdapter : RecyclerView.Adapter
    {
        public event EventHandler<ScrobbleListAdapterClickEventArgs> ItemClick;
        public event EventHandler<ScrobbleListAdapterClickEventArgs> ItemLongClick;
        ScrobbleData[] items;

        public ScrobbleData[] CheckedItems
        {
            get { return items.Where(o => o.IsChecked).ToArray(); }
        }

        public ScrobbleListAdapter(LastTrack[] data, Action<object, ScrobbleListAdapterClickEventArgs> onClick = null)
        {
            items = data.Select(o => new ScrobbleData(o)).ToArray();
            if(onClick != null)
                ItemClick += (o, ea) => onClick(o, ea);
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            var id = Resource.Layout.scrobble_item;
            itemView = LayoutInflater.From(parent.Context).
                   Inflate(id, parent, false);

            var vh = new ScrobbleListAdapterViewHolder(itemView, OnClick, OnLongClick);
            itemView.Tag = vh;
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as ScrobbleListAdapterViewHolder;
            holder.Update(item);
        }

        public override int ItemCount => items.Length;

        public ScrobbleData GetItem(int id)
        {
            if (id < ItemCount && id >= 0)
                return items[id];
            else return null;
        }

        public void SetItems(IList<LastTrack> tracks)
        {
            items = tracks.Select(o => new ScrobbleData(o)).ToArray();
            NotifyDataSetChanged();
        }

        void OnClick(ScrobbleListAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ScrobbleListAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class ScrobbleListAdapterViewHolder : RecyclerView.ViewHolder
    {
        private ScrobbleData _boundTrack;

#pragma warning disable CS0108 // 'ScrobbleListAdapterViewHolder.ItemView' hides inherited member 'RecyclerView.ViewHolder.ItemView'. Use the new keyword if hiding was intended.
        public View ItemView { get; set; }
#pragma warning restore CS0108 // 'ScrobbleListAdapterViewHolder.ItemView' hides inherited member 'RecyclerView.ViewHolder.ItemView'. Use the new keyword if hiding was intended.
        public ImageView CoverImage { get; set; }
        public TextView NameText { get; set; }
        public TextView Timestamp { get; set; }
        public ImageView CheckedImage { get; set; }

        public ScrobbleListAdapterViewHolder(View itemView, Action<ScrobbleListAdapterClickEventArgs> clickListener,
                            Action<ScrobbleListAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            ItemView = itemView;
            CoverImage = itemView.FindViewById<ImageView>(Resource.Id.scrobble_coverImage);
            NameText = itemView.FindViewById<TextView>(Resource.Id.scrobble_name);
            Timestamp = itemView.FindViewById<TextView>(Resource.Id.scrobble_timestamp);
            CheckedImage = ItemView.FindViewById<ImageView>(Resource.Id.scrobble_checked);
            itemView.Click += (sender, e) => clickListener(new ScrobbleListAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new ScrobbleListAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }

        public void Update(ScrobbleData item)
        {
            var track = item.Track;
            _boundTrack = item;
            ItemView.Tag = this;
            var imgUrl = track.Images?.Medium?.AbsoluteUri;
            Picasso.With(CoverImage.Context).Load(imgUrl ?? "https://lastfm-img2.akamaized.net/i/u/64s/4128a6eb29f94943c9d206c08e625904").Into(CoverImage);
            NameText.Text = $"{track.ArtistName} - {track.Name}";
            Timestamp.Text = TimeOffset(track.TimePlayed);
            CheckedImage.Visibility = item.IsChecked ? ViewStates.Visible : ViewStates.Gone;
        }

        public void ChangeChecked()
        {

            _boundTrack.IsChecked = !_boundTrack.IsChecked;
            Update(_boundTrack);
        }

        public string TimeOffset(DateTimeOffset? dto)
        {
            if (dto == null)
                return "N/A";
            var v = dto.Value;
            var diff = DateTimeOffset.Now - v;
            if (diff.TotalSeconds < 60)
                return $"{diff.TotalSeconds:0}s";
            else if (diff.TotalMinutes < 60)
                return $"{diff.Minutes}m";
            else if (diff.TotalHours < 24)
                return $"{diff.Hours}h";
            else if (diff.TotalDays < 30)
                return $"{diff.Days}d";
            else return "Old";
        }

    }

    public class ScrobbleListAdapterClickEventArgs : EventArgs
    {
        public LastTrack Item { get; set; }
        public View View { get; set; }
        public int Position { get; set; }
    }
}