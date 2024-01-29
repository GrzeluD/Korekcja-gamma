using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using GammaHighLevel;
using System.Linq;

namespace Korekcja_gamma
{
    public partial class Gamma : Form
    {
        private Bitmap originalBitmap;
        private Bitmap processedBitmap;
        private double gammaValue = 0.0;
        private int selectedThreads = 1;
        private PictureBox processedPictureBox;
        private System.Windows.Forms.Button downloadButton;
        public List<int> redColorArray = new List<int>();
        public List<int> greenColorArray = new List<int>();
        public List<int> blueColorArray = new List<int>();
        private int check = 0;
        GammaCorrectionC gammaC = new GammaCorrectionC();

        [DllImport(@"C:\Users\alber\source\repos\Korekcja-gamma\Korekcja-gamma\x64\Debug\GammaCorrection.dll")]
        static extern float PixelMod(float[] gammaMask, int[] segmentR, int[] segmentG, int[] segmentB);

        public Gamma()
        {
            InitializeComponent();
        }

        // Funkcja sprawdzająca, czy przycisk przetwarzania powinien być aktywny
        private void CheckButtonEnabledState()
        {
            processImageButton.Enabled = originalBitmap != null && transparencyTrackBar.Value != 0;
            processImageButtonCS.Enabled = originalBitmap != null && transparencyTrackBar.Value != 0;
        }

        // Obsługa kliknięcia przycisku wczytania obrazu
        private void loadImageButton_Click(object sender, EventArgs e)
        {
            // Wybór pliku z obrazem
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All files (*.*)|*.*";

                // Wyświetlenie wybranego obrazu w PictureBox
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox.ImageLocation = openFileDialog.FileName;
                    originalBitmap = new Bitmap(openFileDialog.FileName);
                    pictureBox.Image = originalBitmap;
                    CheckButtonEnabledState();
                }
            }
        }

        // Obsługa zmiany wartości suwaka przezroczystości
        private void transparencyTrackBar_Scroll(object sender, EventArgs e)
        {
            int transparencyValue = transparencyTrackBar.Value;
            transparencyLabel.Text = $"{transparencyValue}";
            CheckButtonEnabledState();
        }

        // Obsługa kliknięcia przycisku przetwarzania obrazu
        private void processImageButton_Click(object sender, EventArgs e)
        {            
            check = 1;
            processedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            var rect = new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height);
            BitmapData originalData = originalBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData processedData = processedBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            float gammaM = transparencyTrackBar.Value;
            if (gammaM < 0) gammaM = (gammaM / 100) * (-1);
            else gammaM = 1 + (gammaM / 100);

            float[] gammaMask = new float[8];
                for (int i = 0; i < gammaMask.Length; i++)
                {
                    gammaMask[i] = gammaM;
                }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int bytesPerPixel = Image.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8;
            int byteCount = originalData.Stride * originalData.Height;
            byte[] pixelData = new byte[byteCount];
            Marshal.Copy(originalData.Scan0, pixelData, 0, byteCount);

            selectedThreads = threatsFromInput();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = selectedThreads };

            
            Parallel.For(0, originalData.Height, parallelOptions, y =>
            {
                int currentLine = y * originalData.Stride;
                for (int x = 0; x < originalData.Width; x += 8)
                {
                    byte[] segmentR = new byte[8];
                    byte[] segmentG = new byte[8];
                    byte[] segmentB = new byte[8];

                    for (int i = 0; i < 8 && x + i < originalData.Width; i++)
                    {
                        int pixelIndex = currentLine + (x + i) * bytesPerPixel;
                        segmentB[i] = pixelData[pixelIndex];
                        segmentG[i] = pixelData[pixelIndex + 1];
                        segmentR[i] = pixelData[pixelIndex + 2];
                    }
                    int[] segmentRInt = Array.ConvertAll(segmentR, b => (int)b);
                    int[] segmentGInt = Array.ConvertAll(segmentG, b => (int)b);
                    int[] segmentBInt = Array.ConvertAll(segmentB, b => (int)b);

                    PixelMod(gammaMask, segmentRInt, segmentGInt, segmentBInt);

                    segmentG = Array.ConvertAll(segmentGInt, b => (byte)b);
                    segmentR = Array.ConvertAll(segmentRInt, b => (byte)b);
                    segmentB = Array.ConvertAll(segmentBInt, b => (byte)b);
                    for (int i = 0; i < 8 && x + i < originalData.Width; i++)
                    {
                        int pixelIndex = currentLine + (x + i) * bytesPerPixel;
                        pixelData[pixelIndex] = segmentB[i];
                        pixelData[pixelIndex + 1] = segmentG[i];
                        pixelData[pixelIndex + 2] = segmentR[i];
                    }
                }
            });
            stopwatch.Stop();
            Marshal.Copy(pixelData, 0, processedData.Scan0, byteCount);
            originalBitmap.UnlockBits(originalData);
            processedBitmap.UnlockBits(processedData);

            
            MessageBox.Show($"Converting time: {stopwatch.ElapsedMilliseconds} ms");
            ShowProcessedBitmap();

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Sprawdź, czy wprowadzony znak to cyfra lub backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // Jeśli nie jest cyfrą ani backspace, zatrzymaj wprowadzanie znaku
                e.Handled = true;
            }
        }

        // Funkcja sprawdzająca ilość wątków z inputa
        /*private int threatsFromInput()
        {
         int maxThreads = Environment.ProcessorCount;
            if (int.TryParse(textBox1.Text, out int userInput))
            {
                if (userInput > maxThreads)
                {
                    // Wpisana liczba jest większa niż maksymalna liczba wątków
                    MessageBox.Show($"Typed number ({userInput}) is higher than max number of threats ({maxThreads}).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show($"Number of threads set: {maxThreads}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedThreads = (int)maxThreads;
                }
                else
                {
                    MessageBox.Show($"Number of threads set: {userInput}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedThreads = (int)userInput;
                }
            }
            else
            {
                // Nic nie jest wpisane w textBox
                MessageBox.Show("Empty thread input. Number of threads set: 1", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedThreads = 1;
            }
            return selectedThreads;
        }*/

        private int threatsFromInput()
        {
            int maxThreads = Environment.ProcessorCount;
            int[] allowedThreadValues = { 64, 32, 16, 8, 4, 2, 1 };

            if (int.TryParse(textBox1.Text, out int userInput) && userInput > 0)
            {
                if (allowedThreadValues.Contains(userInput))
                {
                    if (userInput > maxThreads)
                    {
                        selectedThreads = (int)maxThreads;
                    }
                    else
                    {
                        selectedThreads = (int)userInput;
                    }
                }
                else
                {
                    if (userInput <= maxThreads)
                    {
                        // userInput jest jedną z dozwolonych wartości i nie przekracza maxThreads
                        selectedThreads = (int)userInput;
                    }
                    else
                    {
                        // userInput jest większy niż maxThreads lub nie jest jedną z dozwolonych wartości
                        MessageBox.Show($"Number of threads set: {maxThreads}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        selectedThreads = (int)maxThreads;
                    }
                }
            }
            else
            {
                // Nic nie jest wpisane w textBox
                MessageBox.Show("Empty thread input. Number of threads set: 1", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedThreads = 1;
            }
            return selectedThreads;
        }


        // Funkcja wyświetlająca przetworzony obraz w PictureBox
        private void ShowProcessedBitmap()
        {

            if (downloadButton != null)
            {
                Controls.Remove(downloadButton);
                downloadButton.Dispose();
            }

            if (processedPictureBox != null)
            {
                Controls.Remove(processedPictureBox);
                processedPictureBox.Dispose();
            }
            processedPictureBox = new PictureBox
            {
                Name = "processedPictureBox",
                Size = new Size(pictureBox.Width, pictureBox.Height),
                Location = new Point(pictureBox.Location.X + pictureBox.Width + 20, pictureBox.Location.Y),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = processedBitmap
            };

            downloadButton = new System.Windows.Forms.Button
            {
                Name = "downloadButton",
                Size = new Size(80, 30),
                Location = new Point(pictureBox.Location.X + pictureBox.Width + 200, pictureBox.Location.Y + pictureBox.Height),
                Text = "Download",
            };

            downloadButton.Click += DownloadButton_Click;


            Controls.Add(processedPictureBox);
            Controls.Add(downloadButton);
            downloadButton.BringToFront();
            downloadButton.Show();
            processedPictureBox.BringToFront();
            processedPictureBox.Show();
        }

        private void processImageButtonCS_Click(object sender, EventArgs e)
        {
            gammaValue = transparencyTrackBar.Value;

            gammaValue = gammaValue < 0 ? 1 - ((gammaValue / 100) * (-1)) : (gammaValue / 10);
            Thread[] threads = new Thread[selectedThreads];

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<Task> tasks = new List<Task>();
            selectedThreads = threatsFromInput();
            // Uruchomienie zadania asynchronicznego dla przetwarzania obrazu
            Task imageProcessingTask = Task.Run(() =>
            {
                gammaC.ApplyGammaCorrection(ref processedBitmap, originalBitmap, selectedThreads,(float)gammaValue);
            });

            // Kontynuacja po zakończeniu przetwarzania obrazu
            imageProcessingTask.ContinueWith(task =>
            {
                stopwatch.Stop();
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                MessageBox.Show($"Converting time: {elapsedMilliseconds} ms");

                ShowProcessedBitmap(); // Wyświetlenie przetworzonego obrazu
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            // Tutaj dodaj kod do zapisu przetworzonego obrazu, na przykład w formacie JPEG
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Pliki JPEG (*.jpg)|*.jpg|Wszystkie pliki (*.*)|*.*";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                processedBitmap.Save(saveDialog.FileName, ImageFormat.Jpeg);
                MessageBox.Show("Converted picture has been saved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}