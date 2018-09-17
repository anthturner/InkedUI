using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace InkedUI.Shared
{
    public static class InkedPatterns
    {
        public static InkedPattern White => InkedPattern.CreateSolidPattern(Color.White);
        public static InkedPattern Black => InkedPattern.CreateSolidPattern(Color.Black);
        public static InkedPattern Gray => InkedPattern.CreateDitheredPattern(Color.Black, Color.White);
        public static InkedPattern Red => InkedPattern.CreateSolidPattern(Color.Red);
        public static InkedPattern DarkRed => InkedPattern.CreateDitheredPattern(Color.Red, Color.Black);
        public static InkedPattern LightRed => InkedPattern.CreateDitheredPattern(Color.Red, Color.White);

        public static InkedPattern StripedBlackWhite => InkedPattern.CreateStripedPattern(Color.Black, Color.White);
    }

    public partial class InkedPattern
    {
        public static void PurgeCache() { _cache.Clear(); }
        private static Dictionary<string, Bitmap> _cache = new Dictionary<string, Bitmap>();

        public Pen AsPen() => new Pen(AsBrush());
        public Brush AsBrush() => new TextureBrush(AsBitmap());
        public Bitmap AsBitmap()
        {
            if (!_cache.ContainsKey(Fingerprint))
            {
                var bitmap = new Bitmap(PatternMatrix.GetLength(0), PatternMatrix.GetLength(1));
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                    {
                        bitmap.SetPixel(i, j, Color.FromArgb(
                            PatternMatrix[i, j].R,
                            PatternMatrix[i, j].G,
                            PatternMatrix[i, j].B));
                    }
                _cache.Add(Fingerprint, bitmap);
            }
            return _cache[Fingerprint];
        }

        public int Width => PatternMatrix.GetLength(0);
        public int Height => PatternMatrix.GetLength(1);
        public Color[,] PatternMatrix { get; set; }
        public string Fingerprint
        {
            get
            {
                var bytes = new List<byte>();
                bytes.Add((byte)Width);
                bytes.Add((byte)Height);
                bytes.Add((byte)0xFF);
                var sum = 0;
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                    {
                        var el = PatternMatrix[i, j];
                        bytes.Add((byte)el.R);
                        bytes.Add((byte)el.G);
                        bytes.Add((byte)el.B);
                        bytes.Add((byte)el.A);
                        sum += el.R+el.G+el.B+el.A;
                    }
                bytes.Add((byte)(sum % 255));
                return System.Convert.ToBase64String(bytes.ToArray());
            }
        }
        public static InkedPattern CreateFromFingerprint(string fingerprint)
        {
            var bytes = System.Convert.FromBase64String(fingerprint);
            var width = bytes[0];
            var height = bytes[1];
            var separatorByte = bytes[2];
            var sum = 0;
            var pattern = new InkedPattern() { PatternMatrix = new Color[width, height] };

            var c = 3;
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    var r = bytes[c];
                    var g = bytes[c + 1];
                    var b = bytes[c + 2];
                    var a = bytes[c + 3];
                    c += 4;
                    var color = Color.FromArgb(a, r, g, b);
                    pattern.PatternMatrix[i, j] = color;
                    sum += r + g + b + a;
                }
            var checksum = bytes.LastOrDefault();
            if ((sum % 255) != checksum)
                throw new ArithmeticException($"Pattern checksum error in fingerprint. (Expected {checksum}, got {(sum%255)})");
            return pattern;
        }
        public static InkedPattern CreateSolidPattern(Color color)
        {
            return new InkedPattern { PatternMatrix = new Color[1, 1] { { color } } };
        }

        public static InkedPattern CreateDitheredPattern(Color foreground, Color background)
        {
            var patternArray = new Color[2, 2] {
                { foreground, background },
                { background, foreground }
            };
            return new InkedPattern { PatternMatrix = patternArray };
        }

        public static InkedPattern CreateStripedPattern(Color foreground, Color background)
        {
            var patternArray = new Color[3, 3]
            {
                { foreground, background, background },
                { background, foreground, background },
                { background, background, foreground }
            };
            return new InkedPattern { PatternMatrix = patternArray };
        }
        public static InkedPattern CreateFatStripedPattern(Color foreground, Color background)
        {
            var patternArray = new Color[6, 6]
            {
                { foreground, foreground, foreground, background, background, background },
                { background, foreground, foreground, foreground, background, background },
                { background, background, foreground, foreground, foreground, background },
                { background, background, background, foreground, foreground, foreground },
                { foreground, background, background, background, foreground, foreground },
                { foreground, foreground, background, background, background, foreground },
            };
            return new InkedPattern { PatternMatrix = patternArray };
        }
        public static InkedPattern CreateDiagonalHatchedPattern(Color foreground, Color background)
        {
            var patternArray = new Color[4, 6]
            {
                { foreground, foreground, foreground, background, background, background },
                { background, foreground, foreground, foreground, background, background },
                { background, background, foreground, foreground, foreground, background },
                { background, background, background, foreground, foreground, foreground }
            };
            return new InkedPattern { PatternMatrix = patternArray };
        }
    }
}
