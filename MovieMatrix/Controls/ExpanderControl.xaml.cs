using System.Windows;
using System.Windows.Controls;

namespace MovieMatrix.Controls
{
    public partial class ExpanderControl : RadioButton
    {
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(ExpanderControl));

        public ExpanderControl()
        {
            InitializeComponent();
        }
    }
}
