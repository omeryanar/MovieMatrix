using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MovieMatrix.Controls
{
    public partial class SearchBoxControl : UserControl
    {
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchBoxControl));

        public string EmptySearchText
        {
            get { return (string)GetValue(EmptySearchTextProperty); }
            set { SetValue(EmptySearchTextProperty, value); }
        }
        public static readonly DependencyProperty EmptySearchTextProperty =
            DependencyProperty.Register(nameof(EmptySearchText), typeof(string), typeof(SearchBoxControl));

        public ICommand SearchCommand
        {
            get { return (ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBoxControl));

        public SearchBoxControl()
        {
            InitializeComponent();
        }
    }
}
