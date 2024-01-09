using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Korekcja_gamma
{
    public partial class Gamma : Form
    {
        private Bitmap originalBitmap;
        private Bitmap processedBitmap;
        private double gammaValue = 1.0;
        private int selectedThreads = 1;
        private PictureBox processedPictureBox;

        public Gamma()
        {
            InitializeComponent();
        }

        // Funkcja sprawdzająca, czy przycisk przetwarzania powinien być aktywny
        private void CheckButtonEnabledState()
        {
            processImageButton.Enabled = originalBitmap != null && transparencyTrackBar.Value != 0;
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
            gammaValue = transparencyTrackBar.Value / 10.0;

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
            double red = (double)pixel.R / 255.0;
            double green = (double)pixel.G / 255.0;
            double blue = (double)pixel.B / 255.0;

            double newR = Math.Pow(red, gamma) * 255;
            double newG = Math.Pow(green, gamma) * 255;
            double newB = Math.Pow(blue, gamma) * 255;

            return Color.FromArgb((int)newR, (int)newG, (int)newB);
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

                    // Zapis informacji o pikselach do pliku tekstowego
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                Color pixelColor = originalBitmap.GetPixel(x, y);

                                writer.WriteLine($"Pixel at ({x}, {y}): R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}");
                            }
                        }
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
    }
}