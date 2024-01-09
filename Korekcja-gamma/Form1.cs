using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Korekcja_gamma
{
    public partial class Gamma : Form
    {

        private Bitmap originalBitmap;

        public Gamma()
        {
            InitializeComponent();
        }

        private void CheckButtonEnabledState()
        {
            processImageButton.Enabled = originalBitmap != null && transparencyTrackBar.Value != 0;
        }

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

        private void transparencyTrackBar_Scroll(object sender, EventArgs e)
        {
            int transparencyValue = transparencyTrackBar.Value;
            transparencyLabel.Text = $"{transparencyValue}";
            CheckButtonEnabledState();
        }

        private void processImageButton_Click(object sender, EventArgs e)
        {
            int width = originalBitmap.Width;
            int height = originalBitmap.Height;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files|*.txt";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Utwórz StreamWriter, aby zapisać informacje do pliku
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // Przejdź przez wszystkie piksele obrazu
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // Pobierz kolor piksela
                            Color pixelColor = originalBitmap.GetPixel(x, y);

                            // Zapisz informacje o pikselu do pliku
                            writer.WriteLine($"Pixel at ({x}, {y}): R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}");
                        }
                    }
                }

                MessageBox.Show($"Pixel information saved to {saveFileDialog.FileName}");
            }
        }

        private void Gamma_Load(object sender, EventArgs e)
        {
            int maxThreads = Environment.ProcessorCount;

            for (int i = 1; i <= maxThreads; i++)
            {
                dropDown.Items.Add(i);
            }

            dropDown.SelectedIndex = 0;
        }

        private void transparencyThreadsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedThreads = Convert.ToInt32(dropDown.SelectedItem);
        }
    }
}
