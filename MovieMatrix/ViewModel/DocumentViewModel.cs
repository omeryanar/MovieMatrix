using System;
using DevExpress.Mvvm;

namespace MovieMatrix.ViewModel
{
    public class DocumentViewModel : BaseViewModel
    {
        public Func<string> Header { get; set; }

        public virtual string DocumentId { get; set; }

        public virtual string DocumentType { get; set; }

        public void Close()
        {
            Messenger.Default.Send(new CloseDocumentMessage(this));
        }

        public override string ToString()
        {
            return Header?.Invoke();
        }
    }
}
