using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;


namespace Korekcja_gamma
{
    internal static class Program
    {
        [DllImport(@"C:\Users\alber\Source\Repos\Korekcja-gamma\Korekcja-gamma\x64\Debug\GammaCorrection.dll")]
        static extern float PixelMod(float[] gammaMask, int[] segmentR, int[] segmentG, int[] segmentB);
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Gamma());
        }
    }
}
