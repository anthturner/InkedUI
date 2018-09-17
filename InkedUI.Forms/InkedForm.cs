using InkedUI.Shared;
using System.ComponentModel;
using System.Windows.Forms;

namespace InkedUI.Forms
{
    public class InkedForm : Form
    {
        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkBorderColor { get; set; } = InkedPatterns.Black;

        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkBackgroundColor { get; set; } = InkedPatterns.White;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode) { base.OnPaint(e); return; }
            e.Graphics.FillRectangle(InkBackgroundColor.AsBrush(), e.ClipRectangle);
            e.Graphics.DrawRectangle(InkBorderColor.AsPen(), e.ClipRectangle);
        }
    }
}
