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
        private Bitmap originalBitmap;  // Oryginalny obraz wczytany z pliku
        private Bitmap processedBitmap; // Przetworzony obraz po korekcji gamma
        private double gammaValue = 1.0; // Wartość gamma dla korekcji
        private int selectedThreads = 1; // Liczba wątków (nieużywane w obecnym kodzie)
        private PictureBox processedPictureBox; // PictureBox dla przetworzonego obrazu

        public Gamma()
        {
            InitializeComponent();
        }

        // Sprawdza, czy przycisk przetwarzania powinien być aktywny
        private void CheckButtonEnabledState()
        {
            
            processImageButton.Enabled = originalBitmap != null && transparencyTrackBar.Value != 0;
        }
       
        // Obsługa kliknięcia przycisku wczytania obrazu
        private void loadImageButton_Click(object sender, EventArgs e)
        {
            
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All files (*.*)|*.*";

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
            
            gammaValue = Math.Max(0.1, transparencyTrackBar.Value / 10.0); // Dostosowano wartość minimalną

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

        // Funkcja zapisująca informacje o pikselach do pliku
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

            // Ogranicz wartości do zakresu 0-255
            red = Math.Max(0, Math.Min(255, red));
            green = Math.Max(0, Math.Min(255, green));
            blue = Math.Max(0, Math.Min(255, blue));

            return Color.FromArgb((int)red, (int)green, (int)blue);
        }

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