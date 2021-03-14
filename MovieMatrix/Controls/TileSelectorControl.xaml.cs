using System.Windows;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.LayoutControl;

namespace MovieMatrix.Controls
{
    public partial class TileSelectorControl : TileLayoutControl
    {
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register
            ("SelectedItem", typeof(object), typeof(TileSelectorControl), new PropertyMetadata(null, OnSelectedItemChanged));        

        public event RoutedEventHandler SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }
        public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent
            ("SelectedItemChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(TileSelectorControl));

        public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);

        public TileSelectorControl()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == SelectedItemProperty)
            {
                foreach (var item in LayoutTreeHelper.GetLogicalChildren(this))
                {
                    if (item is Tile tile)
                    {
                        if (tile.DataContext == SelectedItem)
                        {
                            tile.IsMaximized = true;
                            break;
                        }
                    }
                }
            }
            else if (e.Property == MaximizedElementProperty)
                SelectedItem = MaximizedElement.DataContext;
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TileSelectorControl tileSelector = d as TileSelectorControl;
            tileSelector.RaiseEvent(new SelectionChangedEventArgs(SelectedItemChangedEvent, tileSelector.SelectedItem));
        }
    }

    public class SelectionChangedEventArgs : RoutedEventArgs
    {
        public object SelectedItem { get; private set; }

        public SelectionChangedEventArgs(RoutedEvent routedEvent, object selectedItem)
            : base(routedEvent)
        {
            SelectedItem = selectedItem;
        }
    }
}
