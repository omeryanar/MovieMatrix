using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MovieMatrix.Controls
{
    public partial class RadialProgressBar : UserControl
    {
        #region DependencyProperties

        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register(nameof(Min), typeof(double), typeof(RadialProgressBar), new PropertyMetadata(0d, ValueChanged));

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register(nameof(Max), typeof(double), typeof(RadialProgressBar), new PropertyMetadata(10d, ValueChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(RadialProgressBar), new PropertyMetadata(0d, ValueChanged));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
        }
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(double), typeof(RadialProgressBar), new PropertyMetadata(0d));

        public Brush LowValueBrush
        {
            get { return (Brush)GetValue(LowValueBrushProperty); }
            set { SetValue(LowValueBrushProperty, value); }
        }
        public static readonly DependencyProperty LowValueBrushProperty =
            DependencyProperty.Register(nameof(LowValueBrush), typeof(Brush), typeof(RadialProgressBar), new PropertyMetadata(Brushes.Red));

        public Brush MidValueBrush
        {
            get { return (Brush)GetValue(MidValueBrushProperty); }
            set { SetValue(MidValueBrushProperty, value); }
        }
        public static readonly DependencyProperty MidValueBrushProperty =
            DependencyProperty.Register(nameof(MidValueBrush), typeof(Brush), typeof(RadialProgressBar), new PropertyMetadata(Brushes.Yellow));

        public Brush HighValueBrush
        {
            get { return (Brush)GetValue(HighValueBrushProperty); }
            set { SetValue(HighValueBrushProperty, value); }
        }
        public static readonly DependencyProperty HighValueBrushProperty =
            DependencyProperty.Register(nameof(HighValueBrush), typeof(Brush), typeof(RadialProgressBar), new PropertyMetadata(Brushes.Green));

        public ICommand ValueChangedCommand
        {
            get { return (ICommand)GetValue(ValueChangedCommandProperty); }
            set { SetValue(ValueChangedCommandProperty, value); }
        }
        public static readonly DependencyProperty ValueChangedCommandProperty =
            DependencyProperty.Register(nameof(ValueChangedCommand), typeof(ICommand), typeof(RadialProgressBar));

        #endregion

        #region ReadonlyDependencyProperties

        public Brush ValueBrush
        {
            get { return (Brush)GetValue(ValueBrushProperty); }
            protected set { SetValue(ValueBrushPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey ValueBrushPropertyKey = 
            DependencyProperty.RegisterReadOnly(nameof(ValueBrush), typeof(Brush), typeof(RadialProgressBar), new FrameworkPropertyMetadata(Brushes.Red));
        public static readonly DependencyProperty ValueBrushProperty = ValueBrushPropertyKey.DependencyProperty;

        #endregion

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialProgressBar radialProgressBar = d as RadialProgressBar;
            double oldValue = radialProgressBar.Angle;
            double newValue = radialProgressBar.CalculateAngle();

            radialProgressBar.SetValue(AngleProperty, newValue);
            radialProgressBar.Animate(oldValue, newValue);

            if (newValue < 144)
                radialProgressBar.ValueBrush = radialProgressBar.LowValueBrush;
            else if (newValue < 252)
                radialProgressBar.ValueBrush = radialProgressBar.MidValueBrush;
            else
                radialProgressBar.ValueBrush = radialProgressBar.HighValueBrush;

            radialProgressBar.ValueChangedCommand?.Execute(radialProgressBar.Value);
        }

        private double CalculateAngle()
        {
            return 360 * (Value - Min) / (Max - Min);
        }

        private void Animate(double oldValue, double newValue)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation(oldValue, newValue, new Duration(TimeSpan.FromSeconds(2)), FillBehavior.Stop);
            BeginAnimation(AngleProperty, doubleAnimation);
        }

        public RadialProgressBar()
        {
            InitializeComponent();
        }
    }
}
