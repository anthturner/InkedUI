using InkedUI.Shared;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace InkedUI.Forms
{
    public class InkedPatternTypeEditor : System.Drawing.Design.UITypeEditor
    {
        public override void PaintValue(PaintValueEventArgs e) { return; }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context) => false;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var stencil = value as InkedPattern;
            var svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (svc != null)
            {
                using (var form = new StencilEditor())
                {
                    form.Stencil = stencil.PatternMatrix;
                    if (svc.ShowDialog(form) == DialogResult.OK)
                        stencil = new InkedPattern() { PatternMatrix = form.Stencil };
                }
            }
            return stencil;
        }
    }
}
