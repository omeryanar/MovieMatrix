using System;
using DevExpress.Mvvm.POCO;

namespace MovieMatrix.ViewModel
{
    public class MessageViewModel : DialogViewModel
    {
        public virtual string Title { get; set; }

        public virtual string Content { get; set; }

        public virtual string Details { get; set; }

        public virtual MessageType MessageType { get; set; }

        public static MessageViewModel FromException(Exception exception)
        {
            Type exceptionType = exception.GetType();
            string content = String.Format("{0}\n\n{1}: {2}", Properties.Resources.ErrorOccured, exceptionType, exception.Message);
            string details = String.Format("{0}: {1}\n\n{2}", exceptionType, exception.Message, exception.StackTrace);

            Exception innerException = exception.InnerException;
            while (innerException != null)
            {
                details += String.Format("{0}: {1}\n\n{2}\n\n", innerException.GetType(), innerException.Message, innerException.StackTrace);
                innerException = innerException.InnerException;
            }

            MessageViewModel viewModel = ViewModelSource.Create<MessageViewModel>();
            viewModel.Title = Properties.Resources.Error;
            viewModel.MessageType = MessageType.Error;
            viewModel.Content = content;
            viewModel.Details = details;

            return viewModel;
        }
    }
}
