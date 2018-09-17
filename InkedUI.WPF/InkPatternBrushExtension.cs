using InkedUI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace InkedUI.WPF
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public class InkPatternBrushExtension : MarkupExtension
    {
        public enum PatternTypes
        {
            DiagonalHatched,
            DiagonalStripes,
            DiagonalFatStripes,
            Solid,
            Dithered
        }

        public Color Foreground { get; set; } = Colors.Black;
        public Color Background { get; set; } = Colors.Black;
        public PatternTypes PatternType { get; set; } = PatternTypes.Solid;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            switch (PatternType)
            {
                case PatternTypes.DiagonalHatched:
                    return InkedPattern.CreateDiagonalHatchedPattern(
                        Foreground.ToDrawingColor(), 
                        Background.ToDrawingColor()).AsMediaBrushTiled();
                case PatternTypes.DiagonalFatStripes:
                    return InkedPattern.CreateFatStripedPattern(
                        Foreground.ToDrawingColor(),
                        Background.ToDrawingColor()).AsMediaBrushTiled();
                case PatternTypes.DiagonalStripes:
                    return InkedPattern.CreateStripedPattern(
                        Foreground.ToDrawingColor(),
                        Background.ToDrawingColor()).AsMediaBrushTiled();
                case PatternTypes.Dithered:
                    return InkedPattern.CreateDitheredPattern(
                        Foreground.ToDrawingColor(),
                        Background.ToDrawingColor()).AsMediaBrushTiled();
                case PatternTypes.Solid:
                    return InkedPattern.CreateSolidPattern(Foreground.ToDrawingColor()).AsMediaBrushTiled();
            }
            return null;
        }
    }
}
