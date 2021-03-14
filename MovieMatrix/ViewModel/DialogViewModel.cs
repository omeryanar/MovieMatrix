using System.Windows;

namespace MovieMatrix.ViewModel
{
    public class DialogViewModel : BaseViewModel
    {
        public virtual ResizeMode ResizeMode { get; set; } = ResizeMode.CanResize;

        public virtual WindowStyle WindowStyle { get; set; } = WindowStyle.SingleBorderWindow;
    }
}
