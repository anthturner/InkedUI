using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InkedUI.Shared.Dithering
{
    public static class ColorComparisons
    {
        public static Color ClosestByHue(List<Color> Colors, Color target)
        {
            var hue1 = target.GetHue();
            var diffs = Colors.Select(n => GetHueDistance(n.GetHue(), hue1));
            var diffMin = diffs.Min(n => n);
            var idx = diffs.ToList().FindIndex(n => n == diffMin);
            return Colors[idx];
        }

        // closed match in RGB space
        public static Color ClosestByRgbSpace(List<Color> Colors, Color target)
        {
            var ColorDiffs = Colors.Select(n => ColorDiff(n, target)).Min(n => n);
            var idx = Colors.FindIndex(n => ColorDiff(n, target) == ColorDiffs);
            return Colors[idx];
        }

        // weighed distance using hue, saturation and brightness
        public static Color ClosestWeighted(List<Color> Colors, Color target)
        {
            float hue1 = target.GetHue();
            var num1 = ColorNum(target);
            var diffs = Colors.Select(n => Math.Abs(ColorNum(n) - num1) +
                                           GetHueDistance(n.GetHue(), hue1));
            var diffMin = diffs.Min(x => x);
            var idx = diffs.ToList().FindIndex(n => n == diffMin);
            return Colors[idx];
        }

        // Color brightness as perceived:
        private static float GetBrightness(Color c)
        { return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }

        // distance between two hues:
        private static float GetHueDistance(float hue1, float hue2)
        {
            float d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
        }
        
        private static float ColorNum(Color c)
        {
            return c.GetSaturation() * 0.3f + c.GetBrightness() * 0.7f;
        }

        // distance in RGB space
        private static int ColorDiff(Color c1, Color c2)
        {
            return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
                                   + (c1.G - c2.G) * (c1.G - c2.G)
                                   + (c1.B - c2.B) * (c1.B - c2.B));
        }
    }
}
