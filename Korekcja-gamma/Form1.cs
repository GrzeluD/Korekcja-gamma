using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
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
        public List<int> redColorArray = new List<int>();
        public List<int> greenColorArray = new List<int>();
        public List<int> blueColorArray = new List<int>();

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
            float gammaM = transparencyTrackBar.Value;
            if (gammaM < 0) gammaM = (gammaM/100)*(-1);        
            else gammaM=1+(gammaM/100);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float[] gammaMask = new float[8];
            FillColorArraysFromBitmap();
            for (int i = 0; i < gammaMask.Length; i++)
            {
                gammaMask[i] = gammaM;
            }
            //List<int> segmentR = new List<int>();
            //List<int> segmentG = new List<int>();
            //List<int> segmentB = new List<int>();
            int[] segmentR = new int[8];
            int[] segmentG = new int[8];
            int[] segmentB = new int[8];
            //if (redColorArray.Count % 8 == 0) { MessageBox.Show("YES"); }
            int totalLength = redColorArray.Count;
            for (int i = 0; i < redColorArray.Count; i += 8)
            {
                int length = Math.Min(8, totalLength - i);
                /*Array.Copy(redColorArray, 0, segmentR, 0, 8);
                Array.Copy(greenColorArray, 0, segmentG, 0, 8);
                Array.Copy(blueColorArray, 0, segmentB, 0, 8);*/
                redColorArray.CopyTo(i, segmentR, 0, length);
                blueColorArray.CopyTo(i, segmentG, 0, length);
                greenColorArray.CopyTo(i, segmentB, 0, length);
                PixelMod(gammaMask, segmentR, segmentG, segmentB);
                for (int j = 0; j < length; j++)
                {
                    redColorArray[j+i] = segmentR[j];
                    blueColorArray[j+i] = segmentB[j];
                    greenColorArray[j+i] = segmentG[j];
                }
            }

            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            MessageBox.Show($"Czas przetwarzania: {elapsedMilliseconds} ms");
            SavePixelInformationToFile();
            UpdateProcessedBitmapFromColorArrays();
            ShowProcessedBitmap();
        }
        private void FillColorArraysFromBitmap()
        {
            redColorArray.Clear();
            greenColorArray.Clear();
            blueColorArray.Clear();

            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color pixel = originalBitmap.GetPixel(x, y);
                    redColorArray.Add(pixel.R);
                    greenColorArray.Add(pixel.G);
                    blueColorArray.Add(pixel.B);
                }
            }
        }

        private void UpdateProcessedBitmapFromColorArrays()
        {
            processedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            int width = originalBitmap.Width;
            int height = originalBitmap.Height;
            int index = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color newColor = Color.FromArgb(redColorArray[index], greenColorArray[index], blueColorArray[index]);
                    processedBitmap.SetPixel(x, y, newColor);
                    index++;
                }
            }
        }

        // Funkcja wykonująca korekcję gamma na obrazie
        private void ApplyGammaCorrection()
        {
            if (originalBitmap != null)
            {
                processedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                // Iteracja przez piksele obrazu
                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    for (int x = 0; x < originalBitmap.Width; x++)
                    {
                        Color pixel = originalBitmap.GetPixel(x, y);
                        Color newPixel = ApplyGammaToPixel(pixel, gammaValue);
                        processedBitmap.SetPixel(x, y, newPixel);
                    }
                }
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
            gammaValue = transparencyTrackBar.Value;/// 10.0;


            if (gammaValue < 0)
            {
                gammaValue =1-( (gammaValue / 100)* (-1));
            }
            else
            { gammaValue = (gammaValue / 10); }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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