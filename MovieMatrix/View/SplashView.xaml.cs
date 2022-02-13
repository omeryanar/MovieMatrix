using System;
using System.Timers;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;

namespace MovieMatrix.View
{
    public partial class SplashView : UserControl
    {
        public SplashView()
        {
            InitializeComponent();
        }

        public void SetParameter(int framePerSecond = 30, FontFamily fontFamily = null, int fontSize = 16, Brush backgroundBrush = null, Brush textBrush = null, String characterToDisplay = "")
        {
            bool dispRunning = false;
            if (DispatcherTimer.Enabled)
            {
                DispatcherTimer.Stop();
                dispRunning = true;
            }

            if (fontSize > 0)
            {
                RenderingEmSize = fontSize;
            }
            else
            {
                RenderingEmSize = 16;
            }

            if (fontFamily == null)
            {
                if (TextFontFamily == null)
                {
                    TextFontFamily = new FontFamily("Arial");
                }
            }
            else
            {
                TextFontFamily = fontFamily;
            }

            if (characterToDisplay == "")
            {
                AvaiableLetterChars = "abcdefghijklmnopqrstuvwxyz1234567890";
            }
            else
            {
                AvaiableLetterChars = characterToDisplay;
            }

            new Typeface(TextFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal).TryGetGlyphTypeface(out this.GlyphTypeface);
            LetterAdvanceWidth = this.GlyphTypeface.AdvanceWidths[0] * this.RenderingEmSize + 4;
            LetterAdvanceHeight = this.GlyphTypeface.Height * this.RenderingEmSize;
            BaselineOrigin = new Point(2, 2);

            if (backgroundBrush == null)
            {
                if (BackgroundBrush == null)
                {
                    BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 15, 15, 15));
                }
            }
            else
            {
                BackgroundBrush = backgroundBrush;
                BackgroundBrush.Opacity = 1;
            }
            MainCanvas.Background = BackgroundBrush.Clone();
            BackgroundBrush.Opacity = 0.1;

            if (textBrush == null)
            {
                if (TextBrush == null)
                {
                    TextBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                }
            }
            else
            {
                TextBrush = textBrush;
            }

            if (framePerSecond > 0)
            {
                FramePerSecond = framePerSecond;
            }
            else
            {
                if (FramePerSecond == 0)
                {
                    FramePerSecond = 30;
                }
            }

            DispatcherTimer.Interval = 1000D / FramePerSecond;

            Drops = new int[(int)(CanvasRect.Width / LetterAdvanceWidth)];
            for (var x = 0; x < Drops.Length; x++)
            {
                Drops[x] = 1;
            }

            if (dispRunning)
            {
                DispatcherTimer.Start();
            }
        }

        public void Start()
        {
            DispatcherTimer.Start();
        }

        public void Stop()
        {
            DispatcherTimer.Stop();
        }        

        private void Initialize()
        {
            CanvasRect = new Rect(0, 0, MainCanvas.ActualWidth, MainCanvas.ActualHeight);

            RenderTargetBitmap = new RenderTargetBitmap((int)CanvasRect.Width, (int)CanvasRect.Height, 96, 96, PixelFormats.Pbgra32);
            WriteableBitmap = new WriteableBitmap(RenderTargetBitmap);
            MainImage.Source = WriteableBitmap;
            MainCanvas.Measure(new Size(RenderTargetBitmap.Width, RenderTargetBitmap.Height));
            MainCanvas.Arrange(new Rect(new Size(RenderTargetBitmap.Width, RenderTargetBitmap.Height)));

            DispatcherTimer.AutoReset = true;
            DispatcherTimer.Elapsed += DispatcherTimerTick;

            SetParameter(framePerSecond: FramePerSecond, fontSize: (int)RenderingEmSize);
            Drops = new int[(int)(CanvasRect.Width / LetterAdvanceWidth)];
            for (var x = 0; x < Drops.Length; x++)
            {
                Drops[x] = 1;
            }
        }

        private DrawingVisual RenderDrops()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawRectangle(BackgroundBrush, null, CanvasRect);
            List<ushort> glyphIndices = new List<ushort>();
            List<double> advancedWidths = new List<double>();
            List<Point> glyphOffsets = new List<Point>();
            for (var i = 0; i < Drops.Length; i++)
            {
                double x = BaselineOrigin.X + LetterAdvanceWidth * i;
                double y = BaselineOrigin.Y + LetterAdvanceHeight * Drops[i];
                if (y + LetterAdvanceHeight < CanvasRect.Height)
                {
                    var glyphIndex = GlyphTypeface.CharacterToGlyphMap[AvaiableLetterChars[CryptoRandom.Next(0, AvaiableLetterChars.Length - 1)]];
                    glyphIndices.Add(glyphIndex);
                    advancedWidths.Add(0);
                    glyphOffsets.Add(new Point(x, -y));
                }
                if (Drops[i] * LetterAdvanceHeight > CanvasRect.Height && CryptoRandom.NextDouble() > 0.775)
                {
                    Drops[i] = 0;
                }
                Drops[i]++;
            }
            if (glyphIndices.Count > 0)
            {
                float pixelsPerDip = (float)VisualTreeHelper.GetDpi(drawingVisual).PixelsPerDip;
                GlyphRun glyphRun = new GlyphRun(
                                    GlyphTypeface,
                                    0,
                                    false,
                                    RenderingEmSize,
                                    pixelsPerDip,
                                    glyphIndices,
                                    BaselineOrigin,
                                    advancedWidths,
                                    glyphOffsets,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null);
                drawingContext.DrawGlyphRun(TextBrush, glyphRun);
            }
            drawingContext.Close();

            return drawingVisual;
        }

        private void SplashViewLoaded(object sender, RoutedEventArgs e)
        {
            if (!LoadedFlag)
            {
                Window window = Window.GetWindow(this);
                window.Topmost = false;

                LoadedFlag = true;
                Initialize();
            }

            Start();
        }

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                ElapsedEventHandler dt = DispatcherTimerTick;
                Dispatcher.Invoke(dt, sender, e);
                return;
            }

            DrawingVisual dv = RenderDrops();
            RenderTargetBitmap.Render(dv);
            WriteableBitmap.Lock();
            RenderTargetBitmap.CopyPixels(new Int32Rect(0, 0, RenderTargetBitmap.PixelWidth, RenderTargetBitmap.PixelHeight), WriteableBitmap.BackBuffer, WriteableBitmap.BackBufferStride * WriteableBitmap.PixelHeight, WriteableBitmap.BackBufferStride);
            WriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, RenderTargetBitmap.PixelWidth, RenderTargetBitmap.PixelHeight));
            WriteableBitmap.Unlock();
        }

        #region Fields

        private bool LoadedFlag = false;
        private int FramePerSecond = 30;
        private int RenderingEmSize = 16;

        private int[] Drops;
        private Rect CanvasRect;
        private Brush TextBrush;
        private Brush BackgroundBrush;
        private WriteableBitmap WriteableBitmap;
        private RenderTargetBitmap RenderTargetBitmap;

        private GlyphTypeface GlyphTypeface;
        private double LetterAdvanceWidth;
        private double LetterAdvanceHeight;
        private Point BaselineOrigin;

        private Timer DispatcherTimer = new Timer();
        private CryptoRandom CryptoRandom = new CryptoRandom();
        private String AvaiableLetterChars = "abcdefghijklmnopqrstuvwxyz1234567890";
        private FontFamily TextFontFamily = new FontFamily(new Uri("pack://application:,,,"), "./Assets/Font/#Matrix Code NFI");

        #endregion
    }

    public class CryptoRandom : RandomNumberGenerator
    {
        private static RandomNumberGenerator RandomNumberGenerator;

        public CryptoRandom()
        {
            RandomNumberGenerator = Create();
        }

        public override void GetBytes(byte[] buffer)
        {
            RandomNumberGenerator.GetBytes(buffer);
        }

        public double NextDouble()
        {
            byte[] b = new byte[4];
            RandomNumberGenerator.GetBytes(b);

            return (double)BitConverter.ToUInt32(b, 0) / UInt32.MaxValue;
        }

        public int Next(int minPValue, int maxPValue)
        {
            return (int)Math.Round(NextDouble() * (maxPValue - minPValue - 1)) + minPValue;
        }

        public int Next()
        {
            return Next(0, Int32.MaxValue);
        }

        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public override void GetNonZeroBytes(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
