using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.FilteringUI;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Navigation;
using MovieMatrix.Helper;
using MovieMatrix.ViewModel;
using MovieStore;
using MovieStore.Container;
using TMDbLib.Objects.General;

namespace MovieMatrix.Resources
{
    public class TextHighlightingBehavior : Behavior<TextEdit>
    {
        public string HighlightedText
        {
            get { return (string)GetValue(HighlightedTextProperty); }
            set { SetValue(HighlightedTextProperty, value); }
        }
        public static readonly DependencyProperty HighlightedTextProperty = DependencyProperty.Register("HighlightedText", typeof(string), typeof(TextHighlightingBehavior),
            new PropertyMetadata(String.Empty, (obj, e) => 
            { 
                (obj as TextHighlightingBehavior).UpdateText(); 
            }));

        protected void UpdateText()
        {
            if (AssociatedObject == null)
                return;

            if (AssociatedObject.EditMode != EditMode.InplaceInactive)
                return;

            BaseEditHelper.UpdateHighlightingText(AssociatedObject, new TextHighlightingProperties(HighlightedText, FilterCondition.Contains));
        }
    }

    public class FilterEditorBehavior : Behavior<FilterEditorControl>
    {
        public MediaType MediaType
        {
            get { return (MediaType)GetValue(MediaTypeProperty); }
            set { SetValue(MediaTypeProperty, value); }
        }
        public static readonly DependencyProperty MediaTypeProperty = DependencyProperty.Register("MediaType", typeof(MediaType), typeof(FilterEditorBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.QueryFields += AssociatedObject_QueryFields;
            AssociatedObject.QueryOperators += AssociatedObject_QueryOperators;
        }

        private void AssociatedObject_QueryFields(object sender, QueryFieldsEventArgs e)
        {
            e.Fields.Sort((x, y) => x.Caption.ToString().CompareTo(y.Caption.ToString()));
        }

        private void AssociatedObject_QueryOperators(object sender, FilterEditorQueryOperatorsEventArgs e)
        {
            if (e.FieldName == "ImdbRating" || e.FieldName == "PersonalRating" || e.FieldName == "Item.VoteAverage")
            {
                e.DefaultOperator = e.Operators[FilterEditorOperatorType.Greater];
                DataTemplate operandTemplate = AssociatedObject.TryFindResource("RatingTemplate") as DataTemplate;
                if (operandTemplate != null)
                {
                    e.Operators[FilterEditorOperatorType.Equal].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.NotEqual].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.Greater].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.GreaterOrEqual].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.Less].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.LessOrEqual].OperandTemplate = operandTemplate;

                    e.Operators.RemoveAll(x => x.OperatorType == FilterEditorOperatorType.AnyOf || x.OperatorType == FilterEditorOperatorType.NoneOf);
                }

                return;
            }

            IEnumerable<string> uniqueValues = FilterHelper.GetUniqueValues(MediaType, e.FieldName);
            if (uniqueValues != null)
            {
                e.DefaultOperator = e.Operators[FilterEditorOperatorType.Contains];
                RemoveIrrelevantFilters(e.Operators);

                string filterKey = FilterHelper.GetFilterKey(MediaType, e.FieldName);
                DataTemplate operandTemplate = AssociatedObject.TryFindResource(filterKey + "FilterTemplate") as DataTemplate;
                if (operandTemplate != null) 
                {
                    e.Operators[FilterEditorOperatorType.Equal].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.NotEqual].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.Contains].OperandTemplate = operandTemplate;
                    e.Operators[FilterEditorOperatorType.DoesNotContain].OperandTemplate = operandTemplate;
                }
            }
        }

        private void RemoveIrrelevantFilters(FilterEditorOperatorItemList filterEditors)
        {
            filterEditors.RemoveAll(x => x.OperatorType == FilterEditorOperatorType.AnyOf ||
                                         x.OperatorType == FilterEditorOperatorType.NoneOf ||
                                         x.OperatorType == FilterEditorOperatorType.Less ||
                                         x.OperatorType == FilterEditorOperatorType.LessOrEqual ||
                                         x.OperatorType == FilterEditorOperatorType.Greater ||
                                         x.OperatorType == FilterEditorOperatorType.GreaterOrEqual ||
                                         x.OperatorType == FilterEditorOperatorType.Between ||
                                         x.OperatorType == FilterEditorOperatorType.NotBetween);
        }
    }

    public class MiddleClickCloseBehavior : Behavior<OfficeNavigationBar>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed && sender is OfficeNavigationBar navigationBar)
            {
                IInputElement inputElement = navigationBar.InputHitTest(e.GetPosition(navigationBar));
                if (inputElement is FrameworkElement frameworkElement && frameworkElement.DataContext is DocumentViewModel viewModel)
                    viewModel.Close();
            }
        }
    }

    public class FilterPopupBehavior : Behavior<DataViewBase>
    {
        public MediaType MediaType
        {
            get { return (MediaType)GetValue(MediaTypeProperty); }
            set { SetValue(MediaTypeProperty, value); }
        }
        public static readonly DependencyProperty MediaTypeProperty = DependencyProperty.Register("MediaType", typeof(MediaType), typeof(FilterPopupBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ShowFilterPopup += AssociatedObject_ShowFilterPopup;
        }

        private void AssociatedObject_ShowFilterPopup(object sender, FilterPopupEventArgs e)
        {
            PredefinedFilterCollection predefinedFilters = FilterHelper.GetPredefinedFilters(MediaType, e.Column.FieldName);
            if (predefinedFilters != null)
            {
                e.ExcelColumnFilterSettings.FilterItems.Clear();
                foreach (PredefinedFilter predefinedFilter in predefinedFilters)
                    e.ExcelColumnFilterSettings.FilterItems.Add(new CustomComboBoxItem { EditValue = predefinedFilter.Filter, DisplayValue = predefinedFilter.Name });
            }
        }
    }

    public class DepartmentGroupBehavior : Behavior<GridControl>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CustomColumnSort += AssociatedObject_CustomColumnSort;
            
        }

        private void AssociatedObject_CustomColumnSort(object sender, CustomColumnSortEventArgs e)
        {
            if (e.Column.FieldName == "Department")
            {
                string department = (e.Column.DataContext as PersonContainer)?.Person?.KnownForDepartment;
                string value1 = e.Value1.ToString();
                string value2 = e.Value2.ToString();

                if (value1 == value2)
                {
                    e.Result = 0;
                    e.Handled = true;
                }
                else if (value1 == department)
                {
                    e.Result = -1;
                    e.Handled = true;
                }
                else if (value2 == department)
                {
                    e.Result = 1;
                    e.Handled = true;
                }
            }
        }
    }

    public class IntervalGroupBehavior : Behavior<GridControl>
    {
        public const decimal MoneyGroupInterval = 50000000;

        public const decimal MoneyMaxValue = 1000000000;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CustomColumnGroup += AssociatedObject_CustomColumnGroup;
            AssociatedObject.CustomGroupDisplayText += AssociatedObject_CustomGroupDisplayText;
        }

        private void AssociatedObject_CustomGroupDisplayText(object sender, CustomGroupDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "ImdbRating" || e.Column.FieldName == "Item.VoteAverage" || e.Column.FieldName == "PersonalRating")
            {
                int groupValue = Convert.ToInt32(e.Value);
                e.DisplayText = groupValue.ToString();
            }
            else if (e.Column.FieldName == "Item.Budget" || e.Column.FieldName == "Item.Revenue")
            {
                decimal groupValue = Math.Floor(Convert.ToDecimal(e.Value) / MoneyGroupInterval);
                decimal upperBound = (groupValue + 1) * MoneyGroupInterval;
                decimal lowerBound = groupValue * MoneyGroupInterval;

                string groupText = String.Format("{0:#,##0,,M $} - {1:#,##0,,M $} ", lowerBound, upperBound);
                if (lowerBound == 0)
                    groupText = String.Format(" <= {0:#,##0,,M $} ", MoneyGroupInterval);
                if (lowerBound >= MoneyMaxValue)
                    groupText = String.Format(" >= {0:#,##0,,M $} ", MoneyMaxValue);

                e.DisplayText = groupText;
            }
            else if (e.Column.FieldName == "Item.GenreIds" || e.Column.FieldName == "Starring")
            {
                e.DisplayText = e.DisplayText.FirstOfJoinedByListSeparator();
            }
        }

        private void AssociatedObject_CustomColumnGroup(object sender, CustomColumnSortEventArgs e)
        {
            if (e.Column.FieldName == "ImdbRating" || e.Column.FieldName == "Item.VoteAverage" || e.Column.FieldName == "PersonalRating")
            {
                int value1 = Convert.ToInt32(e.Value1);
                int value2 = Convert.ToInt32(e.Value2);

                e.Result = Comparer.Default.Compare(value1, value2);
                e.Handled = true;
            }
            else if (e.Column.FieldName == "Item.Budget" || e.Column.FieldName == "Item.Revenue")
            {
                decimal value1 = Convert.ToDecimal(e.Value1);
                decimal value2 = Convert.ToDecimal(e.Value2);
                decimal valueRate1 = Math.Floor(value1 / MoneyGroupInterval);
                decimal valueRate2 = Math.Floor(value2 / MoneyGroupInterval);

                int result = Comparer.Default.Compare(valueRate1, valueRate2);
                if (value1 > MoneyMaxValue && value2 > MoneyMaxValue)
                    result = 0;

                e.Result = result;
                e.Handled = true;
            }
            else if (e.Column.FieldName == "Item.GenreIds" || e.Column.FieldName == "Starring")
            {
                string value1 = e.Value1.ToString().FirstOfJoinedByListSeparator();
                string value2 = e.Value2.ToString().FirstOfJoinedByListSeparator();

                e.Result = Comparer.Default.Compare(value1, value2);
                e.Handled = true;
            }
        }
    }

    public class RestoreLayoutBehavior : Behavior<GridControl>
    {
        public string RestoreKey
        {
            get { return (string)GetValue(RestoreKeyProperty); }
            set { SetValue(RestoreKeyProperty, value); }
        }
        public static readonly DependencyProperty RestoreKeyProperty =
            DependencyProperty.Register("RestoreKey", typeof(string), typeof(RestoreLayoutBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Initialized += AssociatedObject_Initialized;

            if (LayoutDirectory == null)
                LayoutDirectory = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Layout"));

            LayoutFilePath = Path.Combine(LayoutDirectory.FullName, RestoreKey);
        }

        private void AssociatedObject_Initialized(object sender, EventArgs e)
        {
            Messenger.Default.Register(this, (CloseDocumentMessage message) =>
            {
                if (message.Document is DocumentViewModel document)
                {
                    if (document.DocumentType == RestoreKey)
                        AssociatedObject.SaveLayoutToXml(LayoutFilePath);
                }
            });

            if (File.Exists(LayoutFilePath))
            {
                try
                {
                    AssociatedObject.RestoreLayoutFromXml(LayoutFilePath);
                }
                catch { return; }
            }
        }

        private string LayoutFilePath;

        private static DirectoryInfo LayoutDirectory;
    }

    public class DefaultLayoutBehavior : Behavior<DataViewBase>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.ShowGridMenu += AssociatedObject_ShowGridMenu;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (DefaultLayout == null)
            {
                DefaultLayout = new MemoryStream();
                AssociatedObject.DataControl.SaveLayoutToStream(DefaultLayout);
            }
        }

        private void AssociatedObject_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            BarButtonItem backToDefaultButton = new BarButtonItem { Content = Properties.Resources.DefaultView };
            backToDefaultButton.ItemClick += (x, y) =>
            {
                try
                {
                    DefaultLayout.Position = 0;
                    AssociatedObject.DataControl.RestoreLayoutFromStream(DefaultLayout);
                }
                catch { return; }
            };
           
            e.Customizations.Add(new BarItemSeparator());
            e.Customizations.Add(backToDefaultButton);
        }

        private MemoryStream DefaultLayout;
    }
}
