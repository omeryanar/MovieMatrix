using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MovieMatrix.Model
{
    public class DocumentCollection : ObservableCollection<object>
    {
        public object CurrentDocument
        {
            get { return currentDocument; }
            set
            {
                if (currentDocument != value)
                {
                    currentDocument = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentDocument)));
                }
            }
        }
        private object currentDocument;

        public void ForceRemove(object item)
        {
            forceRemove = true;
            Remove(item);
            forceRemove = false;
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            Reset();
        }

        protected override void InsertItem(int index, object item)
        {
            if (item != lastRemovedDocument)
            {
                base.InsertItem(index, item);
                CurrentDocument = item;
            }
            else
            {
                base.MoveItem(lastRemovedDocumentIndex, index);
                Reset();
            }
        }

        protected override void RemoveItem(int index)
        {
            if (forceRemove)
            {
                base.RemoveItem(index);
                Reset();
            }
            else
            {
                lastRemovedDocumentIndex = index;
                lastRemovedDocument = Items[index];
            }
        }

        private void Reset()
        {
            forceRemove = false;
            lastRemovedDocument = null;
            lastRemovedDocumentIndex = -1;
        }

        private bool forceRemove = false;

        private object lastRemovedDocument = null;

        private int lastRemovedDocumentIndex = -1;
    }
}
