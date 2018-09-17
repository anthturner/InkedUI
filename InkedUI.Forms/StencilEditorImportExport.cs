using InkedUI.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InkedUI.Forms
{
    public partial class StencilEditorImportExport : Form
    {
        public InkedPattern Pattern { get; set; }

        public StencilEditorImportExport()
        {
            InitializeComponent();
            Load += (s, e) =>
            {
                fingerprintCode.Text = Pattern.Fingerprint;
            };
        }

        private void btnGenerateFingerprint_Click(object sender, EventArgs e)
        {
            fingerprintCode.Text = Pattern.Fingerprint;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                var newPattern = InkedPattern.CreateFromFingerprint(fingerprintCode.Text);
                Pattern = newPattern;
                this.Close();
            }
            catch (ArithmeticException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid fingerprint.");
            }
        }
    }
}
