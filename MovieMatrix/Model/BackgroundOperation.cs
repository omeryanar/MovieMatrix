using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace MovieMatrix.Model
{
    public class BackgroundOperation
    {
        public object Operation { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public static void Register(object operation, ICommand cancelCommand)
        {
            BackgroundOperation backgroundOperation = new BackgroundOperation(operation, cancelCommand);
            Operations.Add(backgroundOperation);
        }

        public static CancellationToken Register(object operation, CancellationToken cancellationToken)
        {
            BackgroundOperation backgroundOperation = new BackgroundOperation(operation, cancellationToken);
            Operations.Add(backgroundOperation);

            return backgroundOperation.CancellationTokenSource.Token;
        }

        public static void UnRegister(object operation)
        {
            BackgroundOperation backgroundOperation = Operations.FirstOrDefault(x => x.Operation == operation);
            if (backgroundOperation != null)
                Operations.Remove(backgroundOperation);
        }

        public static ObservableCollection<BackgroundOperation> Operations { get; } = new ObservableCollection<BackgroundOperation>();

        private CancellationTokenSource CancellationTokenSource;

        private BackgroundOperation(object operation, ICommand cancelCommand) 
        {
            Operation = operation;
            CancelCommand = cancelCommand;
        }

        private BackgroundOperation(object operation, CancellationToken cancellationToken)
        {
            Operation = operation;
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancelCommand = new DelegateCommand(() => CancellationTokenSource?.Cancel());
        }
    }
}
