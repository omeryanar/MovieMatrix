using System.Windows;
using DevExpress.Mvvm.UI;
using FirstFloor.ModernUI.Windows.Controls;

namespace Setup.Resources
{
    public interface IModernDialogService
    {
        MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage image);
    }

    public class ModernDialogService : ServiceBase, IModernDialogService
    {
        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            return ModernDialog.ShowMessage(messageBoxText, caption, button, image);
        }
    }
}
