using InkedUI.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InkedUI.Forms
{
    public partial class StencilEditor : Form
    {
        private const int SIZE_X = 250;
        private const int SIZE_Y = 250;

        public Color[,] Stencil { get; set; }

        private int TextureWidth => (int)textureWidth.Value;
        private int TextureHeight => (int)textureHeight.Value;
        private int CellSizeX => SIZE_X / TextureWidth;
        private int CellSizeY => SIZE_Y / TextureHeight;

        public StencilEditor()
        {
            InitializeComponent();
            Stencil = new Color[TextureWidth, TextureHeight];
            for (int x = 0; x < TextureWidth; x++)
                for (int y = 0; y < TextureHeight; y++)
                    Stencil[x, y] = Color.Transparent;
            
            editorImage.MouseDown += (s, e) => DrawCheck(e);
            editorImage.MouseMove += (s, e) => DrawCheck(e);

            UpdateStencil();
        }

        private void DrawCheck(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point coordinates = e.Location;

                var cellX = coordinates.X / CellSizeX;
                var cellY = coordinates.Y / CellSizeY;

                if (cellX >= TextureWidth || cellY >= TextureHeight)
                    return;

                Stencil[cellX, cellY] = GetActiveColor();

                UpdateStencil();
            }
        }
        
        private void UpdateStencil()
        {
            try
            {
                editorImage.Image = GenerateEditorImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            InkedPattern.PurgeCache();
            var pattern = new InkedPattern() { PatternMatrix = Stencil };

            texturePreview.BackgroundImage = new Bitmap(texturePreview.Width, texturePreview.Height);
            using (var g = Graphics.FromImage(texturePreview.BackgroundImage))
                g.FillRectangle(pattern.AsBrush(), 0, 0, texturePreview.Width, texturePreview.Height);

            textPreview.BackgroundImage = new Bitmap(textPreview.Width, textPreview.Height);
            using (var g = Graphics.FromImage(textPreview.BackgroundImage))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.DrawString("Hello, World!", DefaultFont, pattern.AsBrush(), new Point(10, 10));
            }
        }

        private void EditorImage_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;

            var cellX = coordinates.X / CellSizeX;
            var cellY = coordinates.Y / CellSizeY;
            Stencil[cellX, cellY] = GetActiveColor();

            UpdateStencil();
        }

        private Color GetActiveColor()
        {
            if (radioColorBlack.Checked)
                return Color.Black;
            if (radioColorRed.Checked)
                return Color.Red;
            if (radioColorTransparent.Checked)
                return Color.Transparent;
            if (radioColorWhite.Checked)
                return Color.White;
            if (radioColorYellow.Checked)
                return Color.Yellow;
            return Color.Transparent;
        }

        private Bitmap GenerateEditorImage()
        {
            // size here matches size of picturebox!
            var bmp = new Bitmap(SIZE_X, SIZE_Y);
            using (var g = Graphics.FromImage(bmp))
            {
                var cellWidth = bmp.Width / textureWidth.Value;
                var cellHeight = bmp.Height / textureHeight.Value;

                for (int i = 0; i < textureWidth.Value; i++)
                    for (int j = 0; j < textureHeight.Value; j++)
                    {
                        var rect = new Rectangle((int)(i * cellWidth), (int)(j * cellHeight), (int)cellWidth, (int)cellHeight);
                        g.FillRectangle(new SolidBrush(Stencil[i, j]), rect);
                        g.DrawRectangle(Pens.LightGray, rect);
                    }
            }
            return bmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var importExport = new StencilEditorImportExport();
            importExport.Pattern = new InkedPattern() { PatternMatrix = Stencil };
            importExport.ShowDialog(this);
            Stencil = importExport.Pattern.PatternMatrix;
            UpdateStencil();
        }

        private void textureWidth_ValueChanged(object sender, EventArgs e) => UpdateTextureSize();
        private void textureHeight_ValueChanged(object sender, EventArgs e) => UpdateTextureSize();

        private void UpdateTextureSize()
        {
            var currentTexture = (Color[,])Stencil.Clone();
            var newTexture = new Color[TextureWidth, TextureHeight];
            for (int x = 0; x < TextureWidth; x++)
                for (int y = 0; y < TextureHeight; y++)
                    newTexture[x, y] = Color.Transparent;

            var largerX = currentTexture.GetLength(0) > newTexture.GetLength(0) ? currentTexture.GetLength(0) : newTexture.GetLength(0);
            var largerY = currentTexture.GetLength(1) > newTexture.GetLength(1) ? currentTexture.GetLength(1) : newTexture.GetLength(1);
            
            for (int i = 0; i < largerX; i++)
                for (int j = 0; j < largerY; j++)
                {
                    if (currentTexture.GetLength(0) <= i)
                        continue;
                    if (currentTexture.GetLength(1) <= j)
                        continue;
                    if (newTexture.GetLength(0) <= i)
                        continue;
                    if (newTexture.GetLength(1) <= j)
                        continue;
                    newTexture[i, j] = currentTexture[i, j];
                }

            Stencil = newTexture;

            UpdateStencil();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < TextureWidth; i++)
                for (int j = 0; j < TextureHeight; j++)
                    Stencil[i, j] = GetActiveColor();
            UpdateStencil();
        }
    }
}
