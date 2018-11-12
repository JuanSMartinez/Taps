using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Diagnostics;

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
        private static Dictionary<string, int> phonemeList = new Dictionary<string, int>(){
            { "P", 0 },
            { "T", 1 },
            { "K", 2 },
            { "B", 3 },
            { "D", 4 },
            { "G", 5 },
            { "CH", 6},
            { "J", 7 },
            { "F", 8 },
            { "V", 9 },
            { "TH", 10},
            { "DH", 11},
            { "S", 12},
            { "Z", 13},
            { "SH", 14},
            { "ZH", 15},
            { "H", 16 },
            { "M", 17 },
            { "N", 18 },
            { "NG", 19},
            { "L", 20},
            { "R", 21},
            { "W", 22},
            { "Y", 23},
            { "AE", 24},
            { "AH", 25},
            { "OE", 26},
            { "EH", 27},
            { "ER", 28},
            { "IH", 29},
            { "EE", 30},
            { "UH", 31},
            { "OO", 32},
            { "UU", 33},
            { "AW", 34},
            { "AY", 35},
            { "I", 36},
            { "OW", 37},
            { "OY", 38},
            { "DH&UH", 39},
            { "UH&N", 40},
            { "T&UH", 41},
            { "N&D", 42},
            { "S&T", 43},
            { "IH&T", 44},
            { "IH&N", 45},
            { "IH&NG", 46},
            { "N&T", 47},
            { "Y&OO", 48}
        };

        //Path to cygwin with flite compiled and installed
        public string CygwinPath
        {
            get; set;
        }

        //Constructor
        Motu()
        {
            createStructures();
            CygwinPath = "C:\\cygwin64\\bin\\flite.exe";
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
            int phonemeCode;
            if (phonemeList.TryGetValue(phonemeLabel, out phonemeCode))
            {
                play(phonemeCode);
                return true;
            }
            else
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

        //Play a sentence with a certain ICI and IWI value
        public void PlaySentence(string sentence, int ici, int iwi)
        {
            SentencePlayingThread thread = new SentencePlayingThread(ici, iwi, sentence);
            thread.Start();

        }

        //Launc a thread to play a flattened matrix
        private void PlayFlatMatrix(float[] matrix, int width, int height)
        {
            MatrixPlayingThread thread = new MatrixPlayingThread(matrix, width, height);
            thread.Start();
        }

        //Sentence playin thread
        internal class SentencePlayingThread
        {
            private Thread _thread;
            
            private int ICI { get; set; }
            private int IWI { get; set; }
            private string Sentence { get; set; }

            private Dictionary<string, string> fliteMapping;

            public SentencePlayingThread(int ici, int iwi, string sentence)
            {
                ICI = ici;
                IWI = iwi;
                Sentence = sentence;
                InitializeMapping();

            }

            private void InitializeMapping()
            {
                fliteMapping = new Dictionary<string, string>();

                fliteMapping.Add("aa", "UH");
                fliteMapping.Add("ae", "AE");
                fliteMapping.Add("ah", "AH");
                fliteMapping.Add("ao", "AW");
                fliteMapping.Add("aw", "OW");
                fliteMapping.Add("ax", "UH");
                fliteMapping.Add("ay", "I");
                fliteMapping.Add("eh", "EH");
                fliteMapping.Add("el", "UH-L");
                fliteMapping.Add("em", "UH-M");
                fliteMapping.Add("en", "UH-N");
                fliteMapping.Add("er", "ER");
                fliteMapping.Add("ey", "AY");
                fliteMapping.Add("ih", "IH");
                fliteMapping.Add("iy", "EE");
                fliteMapping.Add("ow", "OE");
                fliteMapping.Add("oy", "OY");
                fliteMapping.Add("uh", "UU");
                fliteMapping.Add("uw", "OO");
                fliteMapping.Add("b", "B");
                fliteMapping.Add("ch", "CH");
                fliteMapping.Add("d", "D");
                fliteMapping.Add("dh", "DH");
                fliteMapping.Add("f", "F");
                fliteMapping.Add("g", "G");
                fliteMapping.Add("hh", "H");
                fliteMapping.Add("jh", "J");
                fliteMapping.Add("k", "K");
                fliteMapping.Add("l", "L");
                fliteMapping.Add("m", "M");
                fliteMapping.Add("n", "N");
                fliteMapping.Add("ng", "NG");
                fliteMapping.Add("p", "P");
                fliteMapping.Add("r", "R");
                fliteMapping.Add("s", "S");
                fliteMapping.Add("sh", "SH");
                fliteMapping.Add("t", "T");
                fliteMapping.Add("th", "TH");
                fliteMapping.Add("v", "V");
                fliteMapping.Add("w", "W");
                fliteMapping.Add("y", "Y");
                fliteMapping.Add("z", "Z");
                fliteMapping.Add("zh", "ZH");
            }

            public void Start()
            {
                _thread = new Thread(new ThreadStart(Run));
                _thread.Start();
            }

            private void Run()
            {
                string[] words = Sentence.Split(' ');
                foreach(string word in words)
                {
                    string formattedWord = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());
                    if(formattedWord != null && !formattedWord.Equals(""))
                    {
                        string[] phonemes = GetPhonemeSequenceOf(formattedWord);
                        foreach(string phoneme in phonemes)
                        {
                            string phonemeLabel;
                            if(fliteMapping.TryGetValue(phoneme, out phonemeLabel))
                            {
                                if (phonemeLabel.Contains("-"))
                                {
                                    string[] pair = phonemeLabel.Split('-');
                                    Instance.PlayPhoneme(pair[0]);
                                    Console.WriteLine(pair[0]);
                                    Thread.Sleep(ICI);
                                    Instance.PlayPhoneme(pair[1]);
                                    Console.WriteLine(pair[1]);
                                    Thread.Sleep(ICI);
                                }
                                else
                                {
                                    Instance.PlayPhoneme(phonemeLabel);
                                    Console.WriteLine(phonemeLabel);
                                    Thread.Sleep(ICI);
                                }
                                
                            }
                        }
                    }
                    Thread.Sleep(IWI);
                }
            }

            //Get string sequence of phonemes of a text using flite
            private string[] GetPhonemeSequenceOf(string sentence)
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
              
                startInfo.FileName = Instance.CygwinPath;
                startInfo.Arguments = "/c -t \"" + sentence + "\" -ps -o none";
                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                string[] phonemeSequence = output.Split(' ');
                string[] result = new string[phonemeSequence.Length - 2];
                for (int i = 0; i < phonemeSequence.Length; ++i)
                {
                    if (i != 0 && i != phonemeSequence.Length - 1)
                        result[i-1] = phonemeSequence[i];
                }
                return result;
            }

        }
        
        //Matrix playing thread
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
