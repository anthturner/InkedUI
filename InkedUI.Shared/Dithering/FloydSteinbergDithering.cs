namespace InkedUI.Shared.Dithering
{
    public class FloydSteinbergDithering : DitheringBase
    {
        public FloydSteinbergDithering(FindColor colorfunc) : base(colorfunc) { }

        override protected void PushError(int x, int y, short[] quantError)
        {
            // Push error
            // 			X		7/16
            // 3/16		5/16	1/16
            if (IsValidCoordinate(x+1, y))
                ModifyImageWithErrorAndMultiplier(x+1, y, quantError, 7.0f / 16.0f);

            if (this.IsValidCoordinate(x-1, y+1))
                ModifyImageWithErrorAndMultiplier(x-1, y+1, quantError, 3.0f / 16.0f);

            if (this.IsValidCoordinate(x, y+1))
                ModifyImageWithErrorAndMultiplier(x, y+1, quantError, 5.0f / 16.0f);

            if (this.IsValidCoordinate(x+1, y+1))
                ModifyImageWithErrorAndMultiplier(x+1, y+1, quantError, 1.0f / 16.0f);
        }
    }
}
