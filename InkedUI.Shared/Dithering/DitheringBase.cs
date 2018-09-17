using System;
using System.Drawing;

namespace InkedUI.Shared.Dithering
{
    public delegate Color FindColor(Color original);

    public abstract class DitheringBase
    {
        protected DirectBitmap CurrentBitmap = null; // Slow mode uses this

        protected int Width => CurrentBitmap.Width;
        protected int Height => CurrentBitmap.Height;
        
        protected FindColor TransformationFunction = null;

        public DitheringBase(FindColor transformFunc)
        {
            TransformationFunction = transformFunc;
        }

        // Work horse, call this when you want to dither something
        public Bitmap DoDithering(Bitmap input)
        {
            CurrentBitmap = new DirectBitmap((Bitmap)input.Clone());

            Color originalPixel = Color.White;
            Color newPixel = Color.White;
            short[] quantError = null;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    originalPixel = CurrentBitmap.GetPixel(x, y);
                    newPixel = TransformationFunction(originalPixel);

                    CurrentBitmap.SetPixel(x, y, newPixel);

                    quantError = GetQuantError(originalPixel, newPixel);
                    PushError(x, y, quantError);
                }

            return CurrentBitmap.Bitmap;
        }
        
        protected short[] GetQuantError(Color originalPixel, Color newPixel)
        {
            short[] returnValue = new short[4];

            returnValue[0] = (short)(originalPixel.R - newPixel.R);
            returnValue[1] = (short)(originalPixel.G - newPixel.G);
            returnValue[2] = (short)(originalPixel.B - newPixel.B);
            returnValue[3] = (short)(originalPixel.A - newPixel.A);

            return returnValue;
        }

        protected bool IsValidCoordinate(int x, int y)
        {
            return (0 <= x && x < this.Width && 0 <= y && y < this.Height);
        }

        protected abstract void PushError(int x, int y, short[] quantError);

        public void ModifyImageWithErrorAndMultiplier(int x, int y, short[] quantError, float multiplier)
        {
            Color oldColor = Color.White;
            oldColor = this.CurrentBitmap.GetPixel(x, y);

            Color newColor = Color.FromArgb(
                                GetLimitedValue(oldColor.R, (int)Math.Round(quantError[0] * multiplier)),
                                GetLimitedValue(oldColor.G, (int)Math.Round(quantError[1] * multiplier)),
                                GetLimitedValue(oldColor.B, (int)Math.Round(quantError[2] * multiplier)));

            this.CurrentBitmap.SetPixel(x, y, newColor);
        }

        private static byte GetLimitedValue(byte original, int error)
        {
            int newValue = original + error;
            return (byte)Clamp(newValue, byte.MinValue, byte.MaxValue);
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}