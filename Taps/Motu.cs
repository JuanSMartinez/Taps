using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace Taps
{

    public sealed class Motu
    {
        //Singleton instance
        private static Motu instance = null;

        //Lock object 
        private static readonly object lockObject = new object();

        //DLL imports of the native core library
        [DllImport("MotuCore")]
        static extern void testPlay();
        [DllImport("MotuCore")]
        static extern void createStructures();
        [DllImport("MotuCore")]
        static extern int getLogCode();
        [DllImport("MotuCore")]
        static extern void play(int phonemeCode);
        [DllImport("MotuCore")]
        static extern bool isMotuPlaying();
        [DllImport("MotuCore")]
        static extern void playMatrix(IntPtr matrix, int width, int height);
        [DllImport("MotuCore")]
        static extern void useMotu();
        [DllImport("MotuCore")]
        static extern void useDefaultOutput();

        //Phoneme dictionary
        private static Dictionary<int, string> phonemeList = new Dictionary<int, string>(){
            { 0, "P" },
            { 1, "T" },
            { 2, "K" },
            { 3, "B" },
            { 4, "D" },
            { 5, "G" },
            { 6, "CH"},
            { 7, "J" },
            { 8, "F" },
            { 9, "V" },
            { 10, "TH"},
            { 11, "DH"},
            { 12, "S"},
            { 13, "Z"},
            { 14, "SH"},
            { 15, "ZH"},
            { 16, "H" },
            { 17, "M" },
            { 18, "N" },
            { 19, "NG"},
            { 20, "L"},
            { 21, "R"},
            { 22, "W"},
            { 23, "Y"},
            { 24, "AE"},
            { 25, "AH"},
            { 26, "OE"},
            { 27, "EH"},
            { 28, "ER"},
            { 29, "IH"},
            { 30, "EE"},
            { 31, "UH"},
            { 32, "OO"},
            { 33, "UU"},
            { 34, "AW"},
            { 35, "AY"},
            { 36, "I"},
            { 37, "OW"},
            { 38, "OY"}
        };

        //Constructor
        Motu()
        {
            createStructures();
        }

        //Instance as a property
        public static Motu Instance
        {
            get
            {
                lock(lockObject)
                {
                    if (instance == null)
                        instance = new Motu();
                    return instance;
                }
            }
        }

        //Play a phoneme by the label
        public bool PlayPhoneme(string phonemeLabel)
        {
            foreach (KeyValuePair<int, string> pair in phonemeList)
            {
                if (string.Compare(pair.Value, phonemeLabel.TrimEnd()) == 0)
                {
                    play(pair.Key);
                    return true;
                }
            }
            return false;
        }

        //Play phoneme by code
        public void PlayPhonemeByCode(int code)
        {
            if(code >= 0 && code < phonemeList.Count)
                play(code);
        }

        //Test a sine wave
        public void TestPlay()
        {
            testPlay();
        }

        //use motu as the default playback device
        public void UseMotu()
        {
            useMotu();
        }

        //use the default playback device
        public void UseDefault()
        {
            useDefaultOutput();
        }

        //Is motu currently playing
        public bool IsMotuPlaying()
        {
            return isMotuPlaying();
        }

        //Play a matrix
        public void PlayMatrix(float[,] matrix)
        {
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            float[] flatMatrix = matrix.Cast<float>().ToArray();
            PlayFlatMatrix(flatMatrix, width, height);
        }

        //Launc a thread to play a flattened matrix
        private void PlayFlatMatrix(float[] matrix, int width, int height)
        {
            MatrixPlayingThread thread = new MatrixPlayingThread(matrix, width, height);
            thread.Start();
        }

        internal class MatrixPlayingThread
        {
            private Thread _thread;

            private float[] matrix;
            private int width;
            private int height;

            public MatrixPlayingThread(float[] matrix, int width, int height)
            {
                this.matrix = matrix;
                this.width = width;
                this.height = height;
            }

            public void Start()
            {
                _thread = new Thread(new ThreadStart(this.Run));
                _thread.Start();
            }

            private void Run()
            {
                GCHandle handle = GCHandle.Alloc(this.matrix, GCHandleType.Pinned);
                IntPtr pointer = handle.AddrOfPinnedObject();
                playMatrix(pointer, this.width, this.height);
                while (isMotuPlaying())
                    ;
                handle.Free();
                
            }
        }


    }
    
}
