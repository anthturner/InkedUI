using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace InkedUI.Shared
{
    public class EInkCanvas
    {
        public Color[] AvailableInkColors { get; set; }
        public Bitmap CanvasSurface { get; private set; }
        public int Width => CanvasSurface.Width;
        public int Height => CanvasSurface.Height;
        public event EventHandler<Bitmap> CanvasSurfaceUpdated;

        public EInkCanvas(int width, int height)
        {
            CanvasSurface = new Bitmap(width, height);
        }
        public EInkCanvas(Bitmap existingCanvas)
        {
            CanvasSurface = existingCanvas;
        }

        public void UpdateSurface(Bitmap canvasSurface)
        {
            var oldHash = GetCanvasHash(CanvasSurface);
            var newHash = GetCanvasHash(canvasSurface);

            if (!oldHash.SequenceEqual(newHash))
            {
                CanvasSurface = (Bitmap)canvasSurface.Clone();
                CanvasSurfaceUpdated?.Invoke(this, canvasSurface);
            }
        }

        private byte[] GetCanvasHash(Bitmap canvas)
        {
            var ms = new MemoryStream();
            canvas.Save(ms, ImageFormat.Png);
            return MD5.Create().ComputeHash(ms.ToArray());
        }

        public class JsonStructure
        {
            public int[] Colors { get; set; }
            public byte[] Bitmap { get; set; }
        }

        public static EInkCanvas ImportJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<JsonStructure>(json);
            var bitmapBytes = obj.Bitmap;
            var colors = obj.Colors;

            var bmp = Bitmap.FromStream(new MemoryStream(bitmapBytes));
            var canvas = new EInkCanvas(bmp.Width, bmp.Height);
            canvas.CanvasSurface = (Bitmap)bmp;
            canvas.AvailableInkColors = colors.Select(c => Color.FromArgb(c)).ToArray();
            return canvas;
        }

        public string ExportJson()
        {
            var ms = new MemoryStream();
            CanvasSurface.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);

            return JsonConvert.SerializeObject(new JsonStructure()
            {
                Colors = AvailableInkColors.Select(c => c.ToArgb()).ToArray(),
                Bitmap = ms.ToArray()
            });
        }

        public void ExportDebugKit()
        {
            using (var fs = File.OpenWrite("debug_consolidated.png"))
                ExportPreview(ImageFormat.Png, fs);
            foreach (var color in AvailableInkColors)
                using (var fs = File.OpenWrite($"debug_{color.Name}.bmp"))
                    Export(ImageFormat.Bmp, color, fs);
        }

        public void ExportPreview(ImageFormat imageFormat, Stream consolidatedOutputStream)
        {
            var bmp = GeneratePixelTransformedImage((x, y, inputColor) =>
            {
                // Compares the input color to the given set of available colors, outputs the closest
                var orderedColors = AvailableInkColors.OrderBy(c => CalculateColorDifference(c, inputColor));
                return orderedColors.First();
            });

            bmp.Save(consolidatedOutputStream, imageFormat);
        }

        public void Export(ImageFormat imageFormat, Color filterColor, Stream outputStream)
        {
            Bitmap bmp;
            bmp = GeneratePixelTransformedImage((x, y, inputColor) =>
            {
                try
                {
                    var myColors = AvailableInkColors.ToList().OrderBy(c => CalculateColorDifference(c, inputColor)).ToList();
                    var top = myColors.First();
                    if (CalculateColorDifference(top, filterColor) < 0.5)
                        return Color.Black;
                    return Color.White;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Transformation exception: " + ex.ToString());
                    return Color.White;
                }
            });
            
            bmp.Save(outputStream, imageFormat);
        }

        public void Export(ImageFormat imageFormat, params Stream[] colorStreams)
        {
            if (colorStreams.Length != AvailableInkColors.Length)
                throw new Exception("Must pass in an array of Streams equal in length to the number of colors on the device.");

            for (int i = 0; i < AvailableInkColors.Length; i++)
                Export(imageFormat, AvailableInkColors[i], colorStreams[i]);
        }

        private Bitmap GeneratePixelTransformedImage(Func<int, int, Color, Color> pixelTransformationFunction)
        {
            var cache = new Dictionary<int, Color>();
            var wrappedSurface = new DirectBitmap(CanvasSurface);
            var result = new DirectBitmap(Width, Height);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var px = wrappedSurface.GetPixel(x, y);
                    if (!cache.ContainsKey(px.ToArgb()))
                        cache.Add(px.ToArgb(), pixelTransformationFunction(x, y, wrappedSurface.GetPixel(x, y)));

                    var resultPx = cache[px.ToArgb()];
                    //Color resultPx = pixelTransformationFunction(x, y, wrappedSurface.GetPixel(x, y));
                    if (resultPx != null)
                    {
                        if (resultPx == Color.Transparent)
                            result.SetPixel(x, y, Color.White);
                        else
                            result.SetPixel(x, y, resultPx);
                    }
                }
            return result.Bitmap;
        }

        private Color GetClosestColor(Color[] possible, Color candidate)
        {
            var dict = new Dictionary<Color, int>();
            foreach (var color in possible)
                dict[color] = CalculateColorDifference(color, candidate);
            return dict.OrderBy(d => d.Value).First().Key;
        }

        private int CalculateColorDifference(Color c1, Color c2)
        {
            return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
                                   + (c1.G - c2.G) * (c1.G - c2.G)
                                   + (c1.B - c2.B) * (c1.B - c2.B));
        }
    }
}
