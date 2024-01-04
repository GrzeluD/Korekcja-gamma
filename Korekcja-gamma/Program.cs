using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Korekcja_gamma
{
    internal static class Program
    {

        [DllImport(@"C:\Users\alber\Source\Repos\Korekcja-gamma\Korekcja-gamma\x64\Debug\KorekcjaGamma.dll")]
        static extern int MyProc(int pixel, int gamma);
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Gamma());
        }
    }
}
