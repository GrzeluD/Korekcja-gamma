using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Korekcja_gamma
{
    internal static class Program
    {
        //[DllImport(@"C:\Users\alber\Source\Repos\Korekcja-gamma\Korekcja-gamma\x64\Debug\GammaCorrection.dll")]
        //static extern float PixelMod(float[] gammaMask, int[] segmentR, int[] segmentG, int[] segmentB );
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {        
            {
                float Gamma = 0.25F;
                Random rand = new Random();
                int[] R = new int[256];
                int[] G = new int[256];
                int[] B = new int[256];
                float[] gammaMask = new float[8];
                for (int i = 0; i < 256; i++)
                {
                    R[i] = 200;//rand.Next(0, 254);
                    G[i] = 150;//rand.Next(0, 254);
                    B[i] = 180; // rand.Next(0, 244);
                }
                for (int i = 0; i < gammaMask.Length; i++)
                {
                    gammaMask[i] = Gamma;
                }
                for (int i = 0; i < R.Length; i+=8)
                {
                    int[] segmentR = new int[8];
                    Array.Copy(R, 0, segmentR, 0, 8);                    
                    int[] segmentG = new int[8];
                    Array.Copy(G, 0, segmentG, 0, 8);
                    int[] segmentB = new int[8];
                    Array.Copy(B, 0, segmentB, 0, 8);
                    Console.WriteLine("Przed:" + segmentB[1]);

                    
                   // PixelMod(gammaMask, segmentR, segmentG, segmentB );
                    Console.WriteLine("Po"+segmentB[0]);
                    Console.WriteLine("Po" + segmentB[1]);
                    Console.WriteLine("Po" + segmentB[2]);
                    Console.WriteLine("Po" + segmentB[3]);
                    Console.WriteLine("Po" + segmentB[4]);
                    Console.WriteLine("Po" + segmentB[5]);
                    Console.WriteLine("Po" + segmentB[6]);
                    Console.WriteLine("Po" + segmentB[7]);
                }

                Console.ReadLine();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Gamma());
        }
    }
}
