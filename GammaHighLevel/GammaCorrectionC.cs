using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;


namespace GammaHighLevel
{
    public class GammaCorrectionC
    {

        public void ApplyGammaCorrection(ref Bitmap processedBitmap, Bitmap originalBitmap, int selectedThreads, float gammaValue )
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
                        int i = currentLine + x;
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


                //return processedBitmap;
            }
        }
        private Color ApplyGammaToPixel(Color pixel, double gamma)
        {

            double red = Math.Pow((double)pixel.R / 255.0, gamma) * 255;
            double green = Math.Pow((double)pixel.G / 255.0, gamma) * 255;
            double blue = Math.Pow((double)pixel.B / 255.0, gamma) * 255;

            return Color.FromArgb((int)red, (int)green, (int)blue);

        }

    }
}
