using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Korekcja_gamma
{
    public partial class Gamma : Form
    {
        // Deklaracje zmiennych do przechowywania obrazów, wartości gamma i wątków
        private Bitmap originalBitmap; // Przechowuje oryginalny obraz
        private Bitmap processedBitmap; // Przechowuje obraz po przetworzeniu
        private double gammaValue = 1.0; // Wartość gamma dla korekcji
        private int selectedThreads = 1; // Wybrana liczba wątków

        private PictureBox processedPictureBox; // PictureBox do wyświetlania obrazu przetworzonego

        public Gamma()
        {
            InitializeComponent();
        }

        // Obsługa kliknięcia przycisku "Wczytaj obraz"
        private void loadImageButton_Click(object sender, EventArgs e)
        {
            // Otwiera okno dialogowe do wyboru pliku obrazu
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All files (*.*)|*.*";

                // Jeśli użytkownik wybrał plik i zaakceptował, wczytuje obraz
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Wczytuje obraz i ustawia go jako oryginalny oraz w PictureBox
                    pictureBox.ImageLocation = openFileDialog.FileName;
                    originalBitmap = new Bitmap(openFileDialog.FileName);
                    pictureBox.Image = originalBitmap;
                    CheckButtonEnabledState(); // Sprawdza, czy przycisk jest aktywny
                }
            }
        }

        // Obsługa przesunięcia suwaka przezroczystości
        private void transparencyTrackBar_Scroll(object sender, EventArgs e)
        {
            int transparencyValue = transparencyTrackBar.Value;
            transparencyLabel.Text = $"{transparencyValue}";
            CheckButtonEnabledState(); // Sprawdza, czy przycisk jest aktywny
        }

        // Obsługa zmiany wyboru w ComboBox
        private void dropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedThreads = Convert.ToInt32(dropDown.SelectedItem);
        }

        // Sprawdza, czy przycisk przetwarzania jest aktywny
        private void CheckButtonEnabledState()
        {
            processImageButton.Enabled = originalBitmap != null && transparencyTrackBar.Value != 0;
        }

        // Obsługa kliknięcia przycisku "Przetwórz obraz"
        private void processImageButton_Click(object sender, EventArgs e)
        {
            gammaValue = transparencyTrackBar.Value / 10.0; // Pobiera wartość gamma z suwaka

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Tworzy nowe zadanie asynchroniczne do przetwarzania obrazu
            Task imageProcessingTask = Task.Run(() =>
            {
                ApplyGammaCorrection(); // Wykonuje korekcję gamma
            });

            // Obsługuje kontynuację po zakończeniu zadania przetwarzania obrazu
            imageProcessingTask.ContinueWith(task =>
            {
                stopwatch.Stop();
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                MessageBox.Show($"Czas przetwarzania: {elapsedMilliseconds} ms");

                ShowProcessedBitmap(); // Wyświetla przetworzony obraz
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // Wykonuje korekcję gamma na obrazie
        private void ApplyGammaCorrection()
        {
            if (originalBitmap != null)
            {
                processedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                // Iteruje przez każdy piksel obrazu oryginalnego
                for (int y = 0; y < originalBitmap.Height; y++)
                {
                    for (int x = 0; x < originalBitmap.Width; x++)
                    {
                        Color pixel = originalBitmap.GetPixel(x, y); // Pobiera piksel
                        Color newPixel = ApplyGammaToPixel(pixel, gammaValue); // Stosuje korekcję gamma
                        processedBitmap.SetPixel(x, y, newPixel); // Ustawia piksel w obrazie przetworzonym
                    }
                }
            }
        }

        // Stosuje korekcję gamma do pojedynczego piksela
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

        // Obsługa zdarzenia ładowania okna aplikacji
        private void Gamma_Load(object sender, EventArgs e)
        {
            int maxThreads = Environment.ProcessorCount;

            for (int i = 1; i <= maxThreads; i++)
            {
                dropDown.Items.Add(i);
            }

            dropDown.SelectedIndex = 0;
        }

        // Wyświetla obraz przetworzony w PictureBox
        private void ShowProcessedBitmap()
        {
            // Usuwa poprzedni obraz przetworzony, jeśli istnieje
            if (processedPictureBox != null)
            {
                Controls.Remove(processedPictureBox);
                processedPictureBox.Dispose();
            }

            // Tworzy nowy PictureBox do wyświetlenia obrazu przetworzonego
            processedPictureBox = new PictureBox
            {
                Name = "processedPictureBox",
                Size = new Size(pictureBox.Width, pictureBox.Height),
                Location = new Point(pictureBox.Location.X + pictureBox.Width + 20, pictureBox.Location.Y),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = processedBitmap
            };

            Controls.Add(processedPictureBox); // Dodaje PictureBox do kontrolki Formularza
            processedPictureBox.BringToFront(); // Wyświetla na wierzchu
            processedPictureBox.Show(); // Pokazuje obraz przetworzony
        }
    }
}