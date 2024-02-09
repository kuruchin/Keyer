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

        private bool isProcessing = false;

        public Keyer()
        {
            InitializeComponent();

            pictureBox1.MouseClick += PictureBox_MouseClick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (isProcessing) return;

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
            if (isProcessing) return;

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
            pictureBox2.Image = null;

            processedImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb);

            //ProcessImage();
        }

        private void ProcessImage()
        {
            int tolerance = 200;

            isProcessing = true;

            // Добавляем ProgressBar
            ProgressBar progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = originalImage.Width * originalImage.Height,
                Dock = DockStyle.Bottom
            };

            Controls.Add(progressBar);

            int progress = 0;

            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);

                    if (IsColorSimilar(pixelColor, keyColor, tolerance))
                    {
                        processedImage.SetPixel(x, y, Color.Transparent);
                    }
                    else
                    {
                        processedImage.SetPixel(x, y, pixelColor);
                    }

                    progress++;
                    progressBar.Value = progress;
                    Application.DoEvents();  // Для обновления интерфейса
                }
            }

            pictureBox2.Image = processedImage;
            // Убираем ProgressBar после завершения процесса
            Controls.Remove(progressBar);
            isProcessing = false;
        }

        private bool IsColorSimilar(Color color1, Color color2, int tolerance)
        {
            int deltaR = Math.Abs(color1.R - color2.R);
            int deltaG = Math.Abs(color1.G - color2.G);
            int deltaB = Math.Abs(color1.B - color2.B);

            return (deltaR + deltaG + deltaB) <= tolerance;
        }

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e is MouseEventArgs mouseEventArgs)
            {
                if (!isProcessing && originalImage != null && mouseEventArgs.Button == MouseButtons.Left)
                {
                    // Calculated scaled pixels
                    int scaledX = (int)(mouseEventArgs.X * (originalImage.Width / (double)pictureBox1.Width));
                    int scaledY = (int)(mouseEventArgs.Y * (originalImage.Height / (double)pictureBox1.Height));

                    keyColor = originalImage.GetPixel(scaledX, scaledY);

                    ProcessImage();
                }
            }
        }
    }
}
