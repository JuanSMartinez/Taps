﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taps;

namespace SandBox
{
    class Program
    {

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

        static void Main(string[] args)
        {
            Program prog = new Program();

            Console.WriteLine("Using singleton");
            Motu instance = Motu.Instance;
            //Console.WriteLine("Using default output");
            //instance.UseDefault();
            //Console.WriteLine("TestPlay " + instance.TestPlay());
            //Console.WriteLine("TestPlay 2 " + instance.TestPlay());
            //Console.WriteLine("Play a matrix ");
            //prog.TestMatrixSingleton();
            //prog.TestMatrixSingleton();

            //Console.WriteLine("Calling Flite");
            //Console.WriteLine(instance.GetPhonemeSequenceOf("Hello world. New sentence with a lot of characters to translate"));

            //string testing = "hello world";
            //instance.PlaySentence(testing, 500, 2000);

            instance.PlayPhoneme("IH&NG");
            Console.Read();
        }
    }
}
