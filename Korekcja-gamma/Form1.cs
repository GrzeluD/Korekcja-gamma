using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace Korekcja_gamma
{
    public partial class Gamma : Form
    {
        private Bitmap originalBitmap;
        private Bitmap processedBitmap;
        private double gammaValue = 0.0;
        private int selectedThreads = 1;
        private PictureBox processedPictureBox;
        public List<int> redColorArray = new List<int>();
        public List<int> greenColorArray = new List<int>();
        public List<int> blueColorArray = new List<int>();
        private int check = 0;

        [DllImport(@"C:\Users\alber\Source\Repos\Korekcja-gamma\Korekcja-gamma\x64\Debug\GammaCorrection.dll")]
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

            Marshal.Copy(pixelData, 0, processedData.Scan0, byteCount);
            originalBitmap.UnlockBits(originalData);
            processedBitmap.UnlockBits(processedData);

            stopwatch.Stop();
            MessageBox.Show($"Czas przetwarzania: {stopwatch.ElapsedMilliseconds} ms");
            SavePixelInformationToFile();
            ShowProcessedBitmap();

        }
        private void ApplyGammaCorrection()
        {
            if (originalBitmap != null)
            {
                
                processedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                var rect = new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height);
                BitmapData originalData = originalBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData processedData = processedBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                int bytesPerPixel = 3; // Dla formatu 24bppRgb
                int heightInPixels = originalData.Height;
                int widthInBytes = originalData.Width * bytesPerPixel;
                byte[] pixelData = new byte[originalData.Stride * heightInPixels];

                // Kopiowanie danych pikseli do tablicy
                Marshal.Copy(originalData.Scan0, pixelData, 0, pixelData.Length);

                // Ustawienie opcji Parallel
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = selectedThreads };

                Parallel.For(0, heightInPixels, parallelOptions, y =>
                {
                    int currentLine = y * originalData.Stride;
                    for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        // Odczytanie i przetwarzanie piksela
                        int i =currentLine + x;
                        byte blue = pixelData[i];
                        byte green = pixelData[i + 1];
                        byte red = pixelData[i + 2];
                        Color pixel = Color.FromArgb(red, green, blue);
                        Color newPixel = ApplyGammaToPixel(pixel, gammaValue);

                        pixelData[i] = newPixel.B;
                        pixelData[i + 1] = newPixel.G;
                        pixelData[i + 2] = newPixel.R;
                    }
                });
                // Kopiowanie danych z powrotem do Bitmapy
                Marshal.Copy(pixelData, 0, processedData.Scan0, pixelData.Length);

                originalBitmap.UnlockBits(originalData);
                processedBitmap.UnlockBits(processedData);
            }
        }

        // Funkcja stosująca korekcję gamma do pojedynczego piksela
        private Color ApplyGammaToPixel(Color pixel, double gamma)
        {          

            double red = Math.Pow((double)pixel.R / 255.0, gamma) * 255;
            double green = Math.Pow((double)pixel.G / 255.0, gamma) * 255;
            double blue = Math.Pow((double)pixel.B / 255.0, gamma) * 255;

            return Color.FromArgb((int)red, (int)green, (int)blue);

        }

        // Funkcja obsługująca załadowanie okna aplikacji
        private void Gamma_Load(object sender, EventArgs e)
        {
            int maxThreads = Environment.ProcessorCount;

            // Dodanie liczby wątków do ComboBox
            for (int i = 1; i <= maxThreads; i++)
            {
                dropDown.Items.Add(i);
            }

            dropDown.SelectedIndex = 0;
        }

        // Funkcja zapisująca informacje o pikselach do pliku
        private void SavePixelInformationToFile()
        {
            if (originalBitmap != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files|*.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int width = originalBitmap.Width;
                    int height = originalBitmap.Height;


                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                Color pixelColor = originalBitmap.GetPixel(x, y);

                                redColorArray.Add(pixelColor.R);
                                greenColorArray.Add(pixelColor.G);
                                blueColorArray.Add(pixelColor.B);
                            }
                        }
                    }

                    // Zapis informacji o pikselach do pliku tekstowego
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))

                        foreach (int liczba in redColorArray)
                        {
                            writer.WriteLine($"{liczba}");
                        }

                    MessageBox.Show($"Pixel information saved to {saveFileDialog.FileName}");
                }
            }
        }

        // Funkcja wyświetlająca przetworzony obraz w PictureBox
        private void ShowProcessedBitmap()
        {
            
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
            Controls.Add(processedPictureBox);
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

            // Uruchomienie zadania asynchronicznego dla przetwarzania obrazu
            Task imageProcessingTask = Task.Run(() =>
            {
                ApplyGammaCorrection();
            });

            // Kontynuacja po zakończeniu przetwarzania obrazu
            imageProcessingTask.ContinueWith(task =>
            {
                stopwatch.Stop();
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                MessageBox.Show($"Czas przetwarzania: {elapsedMilliseconds} ms");

                SavePixelInformationToFile(); // Zapis informacji o pikselach do pliku
                ShowProcessedBitmap(); // Wyświetlenie przetworzonego obrazu
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}