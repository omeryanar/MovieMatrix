using System;
using System.ComponentModel;
using LiteDB;
using TMDbLib.Objects.Search;

namespace MovieStore.Container
{
    public abstract class ContainerBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public int Id
        {
            get { return id; }
            protected set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        private int id;

        public SearchBase Item
        {
            get { return item; }
            internal set
            {
                if (item != value)
                {
                    item = value;
                    OnPropertyChanged(nameof(Item));
                }
            }
        }
        private SearchBase item;

        public DateTime DateAdded
        {
            get { return dateAdded; }
            internal set
            {
                if (dateAdded != value)
                {
                    dateAdded = value;
                    OnPropertyChanged(nameof(DateAdded));
                }
            }
        }
        private DateTime dateAdded;

        public DateTime DateUpdated
        {
            get { return dateUpdated; }
            internal set
            {
                if (dateUpdated != value)
                {
                    dateUpdated = value;
                    OnPropertyChanged(nameof(DateUpdated));
                }
            }
        }
        private DateTime dateUpdated;

        public string LocalPath
        {
            get { return localPath; }
            set
            {
                if (localPath != value)
                {
                    localPath = value;
                    OnPropertyChanged(nameof(LocalPath));
                }
            }
        }
        private string localPath;

        [BsonIgnore]
        public bool IsAdded { get => DateAdded != DateTime.MinValue; }

        public ContainerBase() { }

        public ContainerBase(SearchBase item)
        {
            Id = item.Id;
            Item = item;
        }

        public const string ImdbTitlePage = "www.imdb.com/title/";

        public const string FaceBookPage = "www.facebook.com/";

        public const string TwitterPage = "www.twitter.com/";

        public const string InstagramPage = "www.instagram.com/";
    }
}
