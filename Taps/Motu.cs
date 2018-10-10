using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace Taps
{
    public class Motu
    {
        //DLL imports of the native core library
        [DllImport("MotuCore")]
        static extern int testPlay();
        [DllImport("MotuCore")]
        static extern void createStructures();
        [DllImport("MotuCore")]
        static extern int getLogCode();
        [DllImport("MotuCore")]
        static extern int play(int phonemeCode);
        [DllImport("MotuCore")]
        static extern bool isMotuPlaying();
        [DllImport("MotuCore")]
        static extern int playMatrix(IntPtr matrix, int width, int height);
        [DllImport("MotuCore")]
        static extern void useMotu();
        [DllImport("MotuCore")]
        static extern void useDefaultOutput();

        public static void CreateStructures()
        {
            createStructures();
        }

        public static int GetLogCode()
        {
            return getLogCode();
        }

        //Test a sine wave in the 24 channels
        public static int PlaySineTest()
        {
            if (!isMotuPlaying())
                return testPlay();
            else
                return -1;
        }

        //Play a phoneme by code
        public static int PlayPhonemeCode(int code)
        {
            if (!isMotuPlaying())
                return play(code);
            else
                return -1;
        }

        //Play a matrix
        public static int PlayMatrix(float[] matrix, int width, int height)
        {
            if (!isMotuPlaying())
            {
                MatrixPlayingThread thread = new MatrixPlayingThread(matrix, width, height);
                thread.Start();
                return 0;
            }
            else
                return -1;
        }

        //Use Motu
        public static void UseMotu()
        {
            useMotu();
        }

        //Use the default output
        public static void UseDefaultOutput()
        {
            useDefaultOutput();
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
                Console.WriteLine("Free");
            }
        }

    }

    public sealed class Singleton
    {
        private static Singleton instance = null;
        private static readonly object lockObject = new object();

        Singleton()
        {

        }

        public static Singleton Instance
        {
            get
            {
                lock(lockObject)
                {
                    if (instance == null)
                        instance = new Singleton();
                    return instance;
                }
            }
        }


    }
    
}
