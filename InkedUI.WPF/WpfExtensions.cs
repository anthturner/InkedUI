using InkedUI.Shared;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InkedUI.WPF
{
    public static class WpfExtensions
    {
        public static BitmapSource AsBitmapSource(this InkedPattern pattern) =>
            Imaging.CreateBitmapSourceFromHBitmap(
                pattern.AsBitmap().GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

        public static Brush AsMediaBrush(this InkedPattern pattern) =>
            new ImageBrush(pattern.AsBitmapSource());

        public static Brush AsMediaBrushTiled(this InkedPattern pattern) =>
            new ImageBrush(pattern.AsBitmapSource())
            {
                TileMode = TileMode.Tile,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, pattern.Width, pattern.Height)
            };

        internal static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color) =>
            System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
