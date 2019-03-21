using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taps;
using System.Threading;

namespace SandBox
{
    class Program
    {
        public Motu.FinishedPlayingPhonemeCallback myCallback;

        public void TestMatrixSingleton()
        {
            int fs = 44100;
            int durationMillis = 3000;
            int channels = 3;

            //sine waves parameters
            float[] amplitudes = { 0.5f, 0.3f, 0.1f };
            float[] frequencies = { 500f, 2000f, 3000f };

            //The number of rows in the matrix reflects the time duration
            int matrixHeight = (int)(fs * (durationMillis / 1000));
            float[,] matrix = new float[matrixHeight , channels];
            float[] time = new float[matrixHeight];

            for (int t = 0; t < matrixHeight; t++)
                time[t] = (1f * t) / fs;

            for (int i = 0; i < matrixHeight; i++)
                for (int j = 0; j < channels; j++)
                {
                    matrix[i , j] = (float)(amplitudes[j] * Math.Sin(2 * Math.PI * frequencies[j] * time[i]));
                }
            Motu.Instance.PlayMatrix(matrix);
          
        }

        public void Callback(int r)
        {
            Console.WriteLine("Played phoneme with Result " + r);
        }

        public void SentenceCallback(TapsError r)
        {
            Console.WriteLine("Played a sentence with result: " + r);
        }

        public void KnockCallback()
        {
            Console.WriteLine("Played");
        }

        static void Main(string[] args)
        {
            
            Program prog = new Program();
            prog.myCallback = new Motu.FinishedPlayingPhonemeCallback(prog.Callback);

            Console.WriteLine("Using singleton");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Motu instance = Motu.Instance;
            Console.WriteLine("Waiting for initialization ...");
            while (!instance.IsInitialized()) Thread.Sleep(1000);
            stopwatch.Stop();
            long timeMs = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Done after " + timeMs + " ms");
            Motu.Instance.SetPhonemePlayingCallback(prog.Callback);
            Motu.Instance.SetSentencePlayingCallback(prog.SentenceCallback);
            
            //Console.WriteLine("Using default output");
            //instance.UseDefault();
            //Console.WriteLine("TestPlay " + instance.TestPlay());
            //Console.WriteLine("TestPlay 2 " + instance.TestPlay());
            //Console.WriteLine("Play a matrix ");
            //prog.TestMatrixSingleton();
            //prog.TestMatrixSingleton();

            //Console.WriteLine("Calling Flite");
            //Console.WriteLine(instance.GetPhonemeSequenceOf("Hello world. New sentence with a lot of characters to translate"));

            //string testing = "flame";
            //instance.PlaySentence(testing, 75, 500);
            //Console.WriteLine("Next word: ");
            //testing = Console.ReadLine();
            //instance.PlaySentence(testing, 200, 1000);
            //instance.PlayPhoneme("IH&NG");
            //instance.PlayPhoneme("P");
            //instance.PlayPhoneme("KNOCK");

            //instance.PlaySequenceOfPhonemes(new string[] { "M", "OO", "S"}, 150);

            while (true)
            {
                Console.WriteLine("Sentence to translate: ");
                string testing = Console.ReadLine();
                if (testing.Equals("QUIT"))
                    break;
                Console.WriteLine(instance.GetPhonemesOfSentence(testing));
                instance.PlaySentence(testing, 150, 300, prog.KnockCallback);
                
            }
            instance.Dispose();
            Console.WriteLine("Memory disposed");
            Console.Read();
        }
    }
}
