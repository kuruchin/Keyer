using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Keyer
{
    public partial class Keyer : Form
    {
        // Default key color
        private Color keyColor = Color.FromArgb(0, 205, 24);

        // Original image loaded from file
        private Bitmap originalImage;

        // Processed image after color keying
        private Bitmap processedImage;

        // Default tolerance value for color similarity
        private int tolerance = 0;

        // Flag to indicate whether an image processing operation is in progress
        private bool isProcessing = false;

        // Constructor initializes the UI and event handlers
        public Keyer()
        {
            InitializeComponent();

            InitializeUI();
        }

        // Initialize user interface components
        private void InitializeUI()
        {
            // Set initial tolerance label and key color display
            UpdateToleranceLabel(tolerance);
            pictureBox3.BackColor = keyColor;

            // Attach event handlers
            pictureBox1.MouseClick += PictureBoxSelectKeyColor_MouseClick;
            toleranceTrackBar.MouseUp += ToleranceTrackBar_MouseUp;
            toleranceTrackBar.KeyDown += ToleranceTrackBar_KeyDown;
        }

        // Event handler for selecting an image file
        private void SelectPictureButton_Click(object sender, EventArgs e)
        {
            // Check if image processing is in progress
            if (isProcessing) return;

            // Create and configure an OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Files|*.png",
                Title = "Выберите изображение"
            };

            // Display the dialog and load the image if OK is pressed
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadImage(openFileDialog.FileName);
                ProcessImage();
            }
        }

        // Event handler for selecting key color using mouse click on the original image
        private void PictureBoxSelectKeyColor_MouseClick(object sender, MouseEventArgs e)
        {
            // Check conditions for processing and left mouse button click
            if (e is MouseEventArgs mouseEventArgs &&
                !isProcessing &&
                originalImage != null &&
                mouseEventArgs.Button == MouseButtons.Left)
            {
                // Calculate scaled coordinates based on the picture box size
                int scaledX = (int)(mouseEventArgs.X * (originalImage.Width / (double)pictureBox1.Width));
                int scaledY = (int)(mouseEventArgs.Y * (originalImage.Height / (double)pictureBox1.Height));

                // Get the key color from the clicked pixel and update UI
                keyColor = originalImage.GetPixel(scaledX, scaledY);
                pictureBox3.BackColor = keyColor;

                // Process the image with the updated key color
                ProcessImage();
            }
        }

        // Event handler for choosing key color from color palette
        private void ChooseColorFromPalette_Click(object sender, EventArgs e)
        {
            // Check if image processing is in progress
            if (isProcessing) return;

            // Display the ColorDialog with the current key color
            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = pictureBox3.BackColor;

                // Update key color if a new color is selected
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    keyColor = colorDialog.Color;
                    pictureBox3.BackColor = keyColor;

                    // Process the image with the updated key color
                    if (originalImage != null)
                    {
                        ProcessImage();
                    }
                }
            }
        }

        // Event handler for adjusting tolerance using trackbar
        private void ToleranceTrackBar_MouseUp(object sender, EventArgs e)
        {
            TrackBarProceed(sender);
        }

        private void ToleranceTrackBar_Scroll(object sender, EventArgs e)
        {
            TrackBarProceed(sender);
        }

        private void ToleranceTrackBar_ValueChanged(object sender, EventArgs e)
        {
            // Update the tolerance label and process the image
            UpdateToleranceLabel(((TrackBar)sender).Value);

            if (originalImage != null)
            {
                ProcessImage();
            }
        }

        private void TrackBarProceed(object sender)
        {
            // Update the tolerance label and process the image
            UpdateToleranceLabel(((TrackBar)sender).Value);

            if (originalImage != null)
            {
                ProcessImage();
            }
        }

        // Event handler for saving the processed image
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Check if image processing is in progress
            if (isProcessing) return;

            // Save the processed image if available
            if (processedImage != null)
            {
                // Display the SaveFileDialog for saving the processed image
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "PNG Files|*.png";
                    saveFileDialog.Title = "Сохранить обработанное изображение";

                    // Save the image if OK is pressed
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        processedImage.Save(saveFileDialog.FileName, ImageFormat.Png);
                    }
                }
            }
            else
            {
                // Show a warning if no image is available for processing
                MessageBox.Show("Пожалуйста, сначала загрузите изображение для обработки.", "Нет изображения для обработки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Load an image from the specified file path
        private void LoadImage(string filePath)
        {
            originalImage = new Bitmap(filePath);

            pictureBox1.Image = originalImage;
            pictureBox2.Image = null;

            processedImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb);
        }

        // Process the loaded image by replacing key color with transparency
        private void ProcessImage()
        {
            // Sets the processing flag to true and disables trackbar interaction.
            isProcessing = true;
            toleranceTrackBar.Enabled = false;


            // Lock bits for both the original and processed images
            BitmapData originalData = originalImage.LockBits(new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData processedData = processedImage.LockBits(new Rectangle(0, 0, processedImage.Width, processedImage.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int strideOriginal = originalData.Stride;
            int strideProcessed = processedData.Stride;

            unsafe
            {
                byte* originalScan0 = (byte*)originalData.Scan0.ToPointer();
                byte* processedScan0 = (byte*)processedData.Scan0.ToPointer();

                for (int y = 0; y < originalImage.Height; y++)
                {
                    for (int x = 0; x < originalImage.Width; x++)
                    {
                        int originalIndex = y * strideOriginal + x * 4;
                        int processedIndex = y * strideProcessed + x * 4;

                        byte blue = originalScan0[originalIndex];
                        byte green = originalScan0[originalIndex + 1];
                        byte red = originalScan0[originalIndex + 2];
                        byte alpha = originalScan0[originalIndex + 3];

                        Color pixelColor = Color.FromArgb(alpha, red, green, blue);

                        // Check if the color is similar to the key color within the tolerance
                        if (IsColorSimilar(pixelColor, keyColor, tolerance))
                        {
                            // Replace the color with transparency
                            processedScan0[processedIndex] = 0;       // Blue
                            processedScan0[processedIndex + 1] = 0;   // Green
                            processedScan0[processedIndex + 2] = 0;   // Red
                            processedScan0[processedIndex + 3] = 0;   // Alpha
                        }
                        else
                        {
                            // Keep the original color
                            processedScan0[processedIndex] = blue;
                            processedScan0[processedIndex + 1] = green;
                            processedScan0[processedIndex + 2] = red;
                            processedScan0[processedIndex + 3] = alpha;
                        }
                    }
                }
            }

            // Unlock bits for both images
            originalImage.UnlockBits(originalData);
            processedImage.UnlockBits(processedData);

            // Display the processed image
            pictureBox2.Image = processedImage;

            // Resets the processing flag to false and enables trackbar interaction.
            isProcessing = false;
            toleranceTrackBar.Enabled = true;
        }

        // Check if two colors are similar within the specified tolerance
        private bool IsColorSimilar(Color color1, Color color2, int tolerance)
        {
            int deltaR = Math.Abs(color1.R - color2.R);
            int deltaG = Math.Abs(color1.G - color2.G);
            int deltaB = Math.Abs(color1.B - color2.B);

            return (deltaR + deltaG + deltaB) <= tolerance;
        }

        // Update the tolerance label with the specified value
        private void UpdateToleranceLabel(int value)
        {
            tolerance = value;
            trackbarLabel.Text = $"Допуск замены цвета: {tolerance}";
        }

        // Event handler for the KeyDown event of the TrackBar to prevent keyboard control.
        private void ToleranceTrackBar_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
        }
    }
}
