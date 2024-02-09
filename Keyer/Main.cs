using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Keyer
{
    public partial class Keyer : Form
    {
        private Color keyColor = Color.FromArgb(0, 205, 24);
        private Bitmap originalImage;
        private Bitmap processedImage;


        public Keyer()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Files|*.png",
                Title = "Select an Image File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                LoadImage(selectedFilePath);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (processedImage != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Files|*.png",
                    Title = "Save Processed Image"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string savePath = saveFileDialog.FileName;
                    processedImage.Save(savePath, ImageFormat.Png);
                }
            }
            else
            {
                MessageBox.Show("Please load and process an image first.", "No Processed Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadImage(string filePath)
        {
            originalImage = new Bitmap(filePath);

            pictureBox1.Image = originalImage;

            processedImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb);

            ProcessImage();
        }

        private void ProcessImage()
        {
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    processedImage.SetPixel(x, y, (pixelColor == keyColor) ? Color.Transparent : pixelColor);
                }
            }

            pictureBox2.Image = processedImage;
        }
    }
}
