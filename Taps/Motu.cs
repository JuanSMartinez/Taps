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
    //Error codes
    public enum TapsError
    {
        TapsNoError,
        TapsInternalPhonemePlayingError,
        TapsInvalidFirstElementInSentence,
        TapsEmptyQueueSentence
    }

    public sealed class Motu
    {
        //Singleton instance
        private static Motu instance = null;

        //Lock object 
        private static readonly object lockObject = new object();

        //Finished playing callback delegate
        public delegate void FinishedPlayingPhonemeCallback(int result);
        private FinishedPlayingPhonemeCallback externalPhonemePlaybackCallback;
        private FinishedPlayingPhonemeCallback internalPhonemePlaybackCallback;

        //Finished playing a sentence callback
        public delegate void FinishedPlayingSentenceCallback(TapsError result);
        private FinishedPlayingSentenceCallback internalSentencePlaybackCallback;
        private FinishedPlayingSentenceCallback externalSentencePlaybackCallback;

        //Optional callback to signal that a start flag played 
        public delegate void PlayedStartFlagCallback();

        //Sentence players
        private PhonemeSequencePlayer sequencePlayer;
        private SentencePlayer sentencePlayer;

        //Arbitrary matrix player
        private MatrixPlayer matrixPlayer;

        //Initialization flag
        private bool Initialized { get; set; }

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
        [DllImport("MotuCore")]
        static extern void setFinishedPlayingCallback(FinishedPlayingPhonemeCallback callback);
        [DllImport("MotuCore")]
        static extern bool initializationFinished();
        [DllImport("MotuCore")]
        static extern void clearAll();

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
            { "Y&OO", 48},
            { "KNOCK", 49 }
        };

        //Flite phoneme mapping
        private Dictionary<string, string> fliteMapping;

        //Path to cygwin with flite compiled and installed
        public string CygwinPath
        {
            get; set;
        }

        //Constructor
        Motu()
        {
            Initialized = false;
            Thread initilializationThred = new Thread(new ThreadStart(Initialize));
            initilializationThred.Start();
            CygwinPath = "C:\\cygwin64\\bin\\flite.exe";
            InitializeMapping();

        }

        //Instance as a property
        public static Motu Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                        instance = new Motu();
                    return instance;
                }
            }
        }

        //Initialize structures in a thread
        private void Initialize()
        {
            createStructures();
            while (!initializationFinished()) ;
            internalPhonemePlaybackCallback = new FinishedPlayingPhonemeCallback(CallbackHandlerPhoneme);
            internalSentencePlaybackCallback = new FinishedPlayingSentenceCallback(CallbackHandlerSentence);
            sentencePlayer = new SentencePlayer();
            sequencePlayer = new PhonemeSequencePlayer();
            matrixPlayer = new MatrixPlayer();
            Initialized = true;
        }

        //Initialize flite mapping
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

        //Get a phoneme translation
        public string GetPhonemeTranslationFromFlite(string flitePhoneme)
        {
            string result;
            if (fliteMapping.TryGetValue(flitePhoneme, out result))
                return result;
            else
                return "";
        }

        //Is MOTU initialized
        public bool IsInitialized()
        {
            return Initialized;
        }

        //Get the string result of a Taps error
        public string GetErrorStr(TapsError err)
        {
            switch (err)
            {
                case TapsError.TapsNoError:
                    return "No error";
                case TapsError.TapsInternalPhonemePlayingError:
                    return "Internal error while playing an individual phoneme";
                case TapsError.TapsInvalidFirstElementInSentence:
                    return "Invalid type of first phoneme in the sentence";
                case TapsError.TapsEmptyQueueSentence:
                    return "Phoneme transcription of sentence returned an empty sequence";
                default:
                    return "Unknown error code"; 

            }
        }

        //Set external callback for playing phonemes
        public void SetPhonemePlayingCallback(FinishedPlayingPhonemeCallback externalCallback)
        {
            if (Initialized)
            {
                externalPhonemePlaybackCallback = externalCallback;
            }
        }

        //Set external callback for playing sentences
        public void SetSentencePlayingCallback(FinishedPlayingSentenceCallback externalCallback)
        {
            if (Initialized)
            {
                externalSentencePlaybackCallback = externalCallback;
            }
        }


        //Finished playing callback handler
        private void CallbackHandlerPhoneme(int result)
        {
            if (externalPhonemePlaybackCallback != null)
                externalPhonemePlaybackCallback.Invoke(result);
        }

        //Finished playing sentence
        private void CallbackHandlerSentence(TapsError result)
        {
            if (externalSentencePlaybackCallback != null)
                externalSentencePlaybackCallback.Invoke(result);
        }

        //Play a phoneme by the label
        public bool PlayPhoneme(string phonemeLabel)
        {
            if (Initialized)
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
            return false;
        }

        //Play phoneme by code
        public void PlayPhonemeByCode(int code)
        {
            if (Initialized && code >= 0 && code < phonemeList.Count)
                play(code);
        }

        //Test a sine wave
        public void TestPlay()
        {
            if (Initialized)
                testPlay();
        }

        //use motu as the default playback device
        public void UseMotu()
        {
            if (Initialized)
                useMotu();
        }

        //use the default playback device
        public void UseDefault()
        {
            if (Initialized)
                useDefaultOutput();
        }

        //Is motu currently playing
        public bool IsMotuPlaying()
        {
            return Initialized && isMotuPlaying();
        }

        //Play a matrix
        public void PlayMatrix(float[,] matrix)
        {
            if (Initialized)
            {
                int height = matrix.GetLength(0);
                int width = matrix.GetLength(1);
                float[] flatMatrix = matrix.Cast<float>().ToArray();
                PlayFlatMatrix(flatMatrix, width, height);
            }
        }

        //Play a sentence with a certain ICI and IWI value
        public void PlaySentence(string sentence, int ici, int iwi)
        {
            if (Initialized)
            {
                sentencePlayer.Sentence = sentence;
                sentencePlayer.ICI = ici;
                sentencePlayer.IWI = iwi;
                sentencePlayer.StartFlag = false;
                sentencePlayer.Start();
            }

        }

        //Play a sentence with a certain ICI and IWI value and an optional callback to signal that the signal
        public void PlaySentence(string sentence, int ici, int iwi, PlayedStartFlagCallback callback)
        {
            if (Initialized)
            {
                sentencePlayer.Sentence = sentence;
                sentencePlayer.ICI = ici;
                sentencePlayer.IWI = iwi;
                sentencePlayer.StartFlag = true;
                sentencePlayer.OptionalStartFlagCallback = callback;
                sentencePlayer.Start();
            }

        }

        //Play a sequence of phonemes
        public void PlaySequenceOfPhonemes(string[] sequence, int ici)
        {
            if (Initialized)
            {
                sequencePlayer.Index = 0;
                sequencePlayer.ICI = ici;
                sequencePlayer.StartFlag = false;
                sequencePlayer.Start();
            }
        }

        //Play a sequence of phonemes with a signaling callback for when the first phoneme is played
        public void PlaySequenceOfPhonemes(string[] sequence, int ici, PlayedStartFlagCallback callback)
        {
            if (Initialized)
            {
                sequencePlayer.Index = 0;
                sequencePlayer.ICI = ici;
                sequencePlayer.OptionalStartFlagCallback = callback;
                sequencePlayer.StartFlag = true;
                sequencePlayer.Start();
            }
        }

        //Launc a thread to play a flattened matrix
        private void PlayFlatMatrix(float[] matrix, int width, int height)
        {
            if (width > 0 && height > 0)
            {
                matrixPlayer.Matrix = matrix;
                matrixPlayer.Width = width;
                matrixPlayer.Height = height;
                matrixPlayer.Start();
            }
        }

        //Get string sequence of phonemes of a text using flite
        public string[] GetFlitePhonemeSequenceOf(string sentence)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
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
                    result[i - 1] = phonemeSequence[i];
            }
            return result;
        }

        //Get the string phoneme sequence of a sentence
        public string GetPhonemesOfSentence(string sentence)
        {
            string[] words = sentence.Split(' ');
            string result = "";
            foreach (string word in words)
            {
                string formattedWord = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());
                if (formattedWord != null && !formattedWord.Equals(""))
                {
                    string[] phonemes = GetFlitePhonemeSequenceOf(formattedWord);
                    foreach (string phoneme in phonemes)
                    {
                        string phonemeLabel = Instance.GetPhonemeTranslationFromFlite(phoneme);
                        if (!phonemeLabel.Equals(""))
                        {
                            if (phonemeLabel.Contains("-"))
                            {
                                string[] pair = phonemeLabel.Split('-');
                                result += pair[0] + "," + pair[1] + ",";
                            }
                            else
                            {
                                result += phonemeLabel + ",";
                            }

                        }
                    }
                }
                result += "PAUSE" + ",";
            }
            return result.Substring(0, result.Length - 7);
        }

        //Get the string phoneme sequence of a sentence as given by flite
        public string GetPhonemesOfSentenceFlite(string sentence)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = Instance.CygwinPath;
            startInfo.Arguments = "/c -t \"" + sentence + "\" -ps -o none";
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        //Clear all the data initialized
        public void Dispose()
        {
            if (Initialized)
            {
                clearAll();
                Initialized = false;
            }
        }

        //Restart the service if disposed
        public void Restart()
        {
            if (!Initialized)
                Initialize();
        }

        //Sentence playing thread
        internal class SentencePlayer
        {

            public int ICI { get; set; }
            public int IWI { get; set; }
            public string Sentence { get; set; }
            public bool StartFlag { get; set; }
            public PlayedStartFlagCallback OptionalStartFlagCallback { get; set; }
            private Queue<QueuedElement> queue;
            private bool Running { get; set; }
            private FinishedPlayingPhonemeCallback syncCallbackInstance;
            private FinishedPlayingPhonemeCallback previousCallback;
            private Thread _thread;
            private int phonemesPlayed;

            public SentencePlayer()
            {
                ICI = 150;
                IWI = 300;
                StartFlag = false;
                Sentence = "";
                Running = false;
                phonemesPlayed = 0;
                previousCallback = Instance.internalPhonemePlaybackCallback;
                syncCallbackInstance = new FinishedPlayingPhonemeCallback(CallbackHandler);
                OptionalStartFlagCallback = null;
                queue = new Queue<QueuedElement>();

            }

            private void FillQueue()
            {
                string[] words = Sentence.Split(' ');
                if (StartFlag)
                    queue.Enqueue(new QueuedElement("KNOCK", QueuedElement.types.start));
                foreach (string word in words)
                {
                    string formattedWord = new string(word.Where(c => !char.IsPunctuation(c)).ToArray());
                    if (formattedWord != null && !formattedWord.Equals(""))
                    {
                        string[] phonemes = Instance.GetFlitePhonemeSequenceOf(formattedWord);
                        foreach (string phoneme in phonemes)
                        {
                            string phonemeLabel = Instance.GetPhonemeTranslationFromFlite(phoneme);
                            if (!phonemeLabel.Equals(""))
                            {
                                if (phonemeLabel.Contains("-"))
                                {
                                    string[] pair = phonemeLabel.Split('-');
                                    queue.Enqueue(new QueuedElement(pair[0], QueuedElement.types.phoneme));
                                    queue.Enqueue(new QueuedElement(pair[1], QueuedElement.types.phoneme));
                                }
                                else
                                {
                                    queue.Enqueue(new QueuedElement(phonemeLabel, QueuedElement.types.phoneme));
                                }

                            }
                        }
                    }
                    queue.Enqueue(new QueuedElement("Pause", QueuedElement.types.wordPause));
                }
            }

            public void Start()
            {
                Running = false;
                phonemesPlayed = 0;
                _thread = new Thread(new ThreadStart(Run));
                _thread.Start();
            }

            private void Run()
            {
                FillQueue();
                setFinishedPlayingCallback(syncCallbackInstance);
                try
                {
                    QueuedElement first = queue.Dequeue();
                    if (first.Type == QueuedElement.types.phoneme || first.Type == QueuedElement.types.start)
                    {
                        Instance.PlayPhoneme(first.Symbol);
                        while (true) ;
                    }
                    else
                    {
                        Instance.internalSentencePlaybackCallback.Invoke(TapsError.TapsInvalidFirstElementInSentence);
                        return;
                    }

                }
                catch (InvalidOperationException emptyException)
                {
                    Instance.internalSentencePlaybackCallback.Invoke(TapsError.TapsEmptyQueueSentence);
                    return;
                }
            }

            private void CallbackHandler(int result)
            {
                if (result == 0)
                {
                    phonemesPlayed++;
                    try
                    {
                        QueuedElement element = queue.Dequeue();
                        if (element.Type == QueuedElement.types.phoneme)
                        {
                            Thread.Sleep(ICI);
                            if (phonemesPlayed == 1 && StartFlag)
                                OptionalStartFlagCallback?.Invoke();
                            Instance.PlayPhoneme(element.Symbol);
                        }
                        else
                        {
                            Thread.Sleep(IWI);
                            CallbackHandler(0);
                        }
                    }
                    catch (InvalidOperationException emptyException)
                    {
                        setFinishedPlayingCallback(previousCallback);
                        Instance.internalSentencePlaybackCallback.Invoke(TapsError.TapsNoError);
                        _thread?.Abort();
                    }
                }
                else
                {
                    setFinishedPlayingCallback(previousCallback);
                    Instance.internalSentencePlaybackCallback.Invoke(TapsError.TapsInternalPhonemePlayingError);
                    _thread?.Abort();
                }
            }

            internal class QueuedElement
            {
                public enum types { wordPause, phoneme, start };
                public types Type { get; set; }
                public string Symbol { get; set; }
                public QueuedElement(string symbol, types type)
                {
                    Type = type;
                    Symbol = symbol;
                }
            }
        }

        //Matrix playing thread
        internal class MatrixPlayer
        {
            private Thread _thread;

            public float[] Matrix;
            public int Width;
            public int Height;
            private FinishedPlayingPhonemeCallback syncCallbackInstance;
            private GCHandle handle;
            private IntPtr pointer;

            public MatrixPlayer()
            {
                syncCallbackInstance = new FinishedPlayingPhonemeCallback(CallbackHandler);
             
                
            }

            public void Start()
            {
                _thread = new Thread(new ThreadStart(Run));
                _thread.Start();
            }

            private void Run()
            {
                setFinishedPlayingCallback(syncCallbackInstance);
                handle = GCHandle.Alloc(Matrix, GCHandleType.Pinned);
                pointer = handle.AddrOfPinnedObject();
                playMatrix(pointer, Width, Height);
                while (true)
                    ;

            }

            private void CallbackHandler(int result)
            {
                setFinishedPlayingCallback(Instance.internalPhonemePlaybackCallback);
                Instance.internalPhonemePlaybackCallback(result);
                handle.Free();
                _thread?.Abort();
    
            }
        }

        //Phoneme Sequence playing thread
        internal class PhonemeSequencePlayer
        {
            public int ICI { get; set; }
            public string[] PhonemeSequence { get; set; }
            public int Index{get; set; }
            public bool StartFlag { get; set; }
            public PlayedStartFlagCallback OptionalStartFlagCallback { get; set; }

            private FinishedPlayingPhonemeCallback syncCallbackInstance;
            private FinishedPlayingPhonemeCallback previousCallback;
            private Thread _thread;
            private int phonemesPlayed;

            public PhonemeSequencePlayer()
            {
                ICI = 150;
                PhonemeSequence = null;
                syncCallbackInstance = new FinishedPlayingPhonemeCallback(CallbackHandler);
                Index = 0;
                phonemesPlayed = 0;
                StartFlag = false;
                OptionalStartFlagCallback = null;
                previousCallback = Instance.internalPhonemePlaybackCallback;
            }

            public void Start()
            {
                Index = 0;
                phonemesPlayed = 0;
                _thread = new Thread(new ThreadStart(Run));
                _thread.Start();
            }

            private void Run()
            {
                try
                {
                    string first = PhonemeSequence[Index];
                    setFinishedPlayingCallback(syncCallbackInstance);
                    Instance.PlayPhoneme(first);
                    while (true) ;
                }
                catch (Exception emptyException)
                {
                    Instance.internalSentencePlaybackCallback.Invoke(TapsError.TapsEmptyQueueSentence);
                }
            }

            private void CallbackHandler(int result)
            {
                if (result == 0)
                {
                    phonemesPlayed++;
                    try
                    {
                        Index += 1;
                        string phoneme = PhonemeSequence[Index];
                        Thread.Sleep(ICI);
                        if (phonemesPlayed == 1 && StartFlag)
                            OptionalStartFlagCallback?.Invoke();
                        Instance.PlayPhoneme(phoneme);
                    }
                    catch (Exception e)
                    {
                        setFinishedPlayingCallback(previousCallback);
                        _thread?.Abort();
                        Instance.internalSentencePlaybackCallback.Invoke(TapsError.TapsNoError);
                    }
                }
                else
                {
                    setFinishedPlayingCallback(previousCallback);
                    _thread?.Abort();
                    Instance.internalSentencePlaybackCallback.Invoke(TapsError.TapsInternalPhonemePlayingError);
                }
            }
        }

        
    }
    
}
