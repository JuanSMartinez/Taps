using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taps;

namespace SandBox
{
    class Program
    {

        public void TestMatrix()
        {
            int fs = 44100;
            int durationMillis = 3000;
            int channels = 3;

            //sine waves parameters
            float[] amplitudes = { 0.5f, 0.3f, 0.1f };
            float[] frequencies = { 100f, 200f, 300f };

            //The number of rows in the matrix reflects the time duration
            int matrixHeight = (int)(fs * (durationMillis / 1000));
            float[] matrix = new float[matrixHeight * channels];
            float[] time = new float[matrixHeight];

            for (int t = 0; t < matrixHeight; t++)
                time[t] = (1f * t) / fs;

            for (int i = 0; i < matrixHeight; i++)
                for (int j = 0; j < channels; j++)
                {
                    matrix[i * channels + j] = (float)(amplitudes[j] * Math.Sin(2 * Math.PI * frequencies[j] * time[i]));
                }
            
            Motu.PlayMatrix(matrix, channels, matrixHeight);
            Console.WriteLine("Sent to play");
        }
        static void Main(string[] args)
        {
            Program prog = new Program();
            Motu.CreateStructures();
            Console.WriteLine("Creation"  );
            Console.WriteLine("Use default output");
            Motu.UseDefaultOutput();
            Console.WriteLine("Play test sine " + Motu.PlaySineTest());
            //Console.WriteLine("Play OO " + Motu.PlayPhonemeCode(32));
            //Console.WriteLine("Play a matrix ");
            //prog.TestMatrix();
            Console.Read();
        }
    }
}
