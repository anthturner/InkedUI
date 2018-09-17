using InkedUI.Shared;
using System.ComponentModel;
using System.Drawing.Text;
using System.Windows.Forms;

namespace InkedUI.Forms
{
    public class InkedCheckBox : CheckBox
    {
        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkEmptyCheckBoxBorder { get; set; } = InkedPatterns.Black;

        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkEmptyCheckBox { get; set; } = InkedPatterns.White;

        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkCheckedCheckBoxBorder { get; set; } = InkedPatterns.Black;

        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkCheckedCheckBox { get; set; } = InkedPatterns.White;

        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkCheckedCheckBoxInner { get; set; } = InkedPatterns.Red;

        [Browsable(true)]
        [Category("InkedUI")]
        [Editor(typeof(InkedPatternTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public InkedPattern InkTextColor { get; set; } = InkedPatterns.Black;

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (DesignMode) { base.OnPaint(pe); return; }

            var checkboxLocation = new System.Drawing.Rectangle(0, 0, 10, 10);
            checkboxLocation.Offset(pe.ClipRectangle.Location);
            var innerLocation = new System.Drawing.Rectangle(3, 3, 5, 5);
            innerLocation.Offset(pe.ClipRectangle.Location);
            var textLocation = new System.Drawing.Point(pe.ClipRectangle.Location.X, pe.ClipRectangle.Location.Y);
            textLocation.Offset(15, 0);

            if (this.Checked)
            {
                pe.Graphics.FillRectangle(InkCheckedCheckBox.AsBrush(), checkboxLocation);
                pe.Graphics.FillRectangle(InkCheckedCheckBoxInner.AsBrush(), innerLocation);
                pe.Graphics.DrawRectangle(InkCheckedCheckBoxBorder.AsPen(), checkboxLocation);
            }
            else
            {
                pe.Graphics.FillRectangle(InkEmptyCheckBox.AsBrush(), checkboxLocation);
                pe.Graphics.DrawRectangle(InkEmptyCheckBoxBorder.AsPen(), checkboxLocation);
            }

            pe.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            pe.Graphics.DrawString(this.Text, this.Font, InkTextColor.AsBrush(), textLocation);
        }
    }
}
