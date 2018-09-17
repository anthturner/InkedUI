using InkedUI.Shared.Dithering;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InkedUI.Forms
{
    public class InkedPictureBox : PictureBox
    {
        public enum DitherModes
        {
            FloydSteinberg
        }

        [Browsable(true)]
        [Description("The strategy to use when dithering the image for display on e-ink.")]
        public DitherModes DitherMode { get; set; }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (DesignMode) { base.OnPaint(pe); return; }
            if (this.Image == null)
                return;

            var registeredControl = this.GetRegisteredControl();
            if (registeredControl == null)
                return;

            // Perform resize first
            var resized = new Bitmap(pe.ClipRectangle.Width, pe.ClipRectangle.Height);
            using (var g = Graphics.FromImage(resized))
                g.DrawImage(Image, pe.ClipRectangle, new Rectangle(new Point(0,0), Image.Size), GraphicsUnit.Pixel);

            var dithering = new FloydSteinbergDithering((color) => ColorComparisons.ClosestByRgbSpace(registeredControl.Canvas.AvailableInkColors.ToList(), color));
            var dithered = dithering.DoDithering((Bitmap)resized);
            pe.Graphics.DrawImage(dithered, pe.ClipRectangle.Location);
        }
    }
}
