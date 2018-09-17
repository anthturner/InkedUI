namespace InkedUI.Forms
{
    partial class StencilEditorImportExport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fingerprintCode = new System.Windows.Forms.TextBox();
            this.btnGenerateFingerprint = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fingerprintCode
            // 
            this.fingerprintCode.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fingerprintCode.Location = new System.Drawing.Point(13, 13);
            this.fingerprintCode.Multiline = true;
            this.fingerprintCode.Name = "fingerprintCode";
            this.fingerprintCode.Size = new System.Drawing.Size(340, 85);
            this.fingerprintCode.TabIndex = 0;
            // 
            // btnGenerateFingerprint
            // 
            this.btnGenerateFingerprint.Location = new System.Drawing.Point(13, 104);
            this.btnGenerateFingerprint.Name = "btnGenerateFingerprint";
            this.btnGenerateFingerprint.Size = new System.Drawing.Size(157, 23);
            this.btnGenerateFingerprint.TabIndex = 1;
            this.btnGenerateFingerprint.Text = "Generate Fingerprint";
            this.btnGenerateFingerprint.UseVisualStyleBackColor = true;
            this.btnGenerateFingerprint.Click += new System.EventHandler(this.btnGenerateFingerprint_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(196, 104);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(157, 23);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // StencilEditorImportExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 137);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnGenerateFingerprint);
            this.Controls.Add(this.fingerprintCode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StencilEditorImportExport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import/Export Stencil";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox fingerprintCode;
        private System.Windows.Forms.Button btnGenerateFingerprint;
        private System.Windows.Forms.Button btnImport;
    }
}