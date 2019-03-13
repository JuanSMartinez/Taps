#include "Handlers.h"

namespace Handlers
{
	std::atomic<int> threadsFinished = 0;

	void initializeSubsetOfPhonemes(Phoneme* phonemeArray, int min, int max)
	{
		for (int i = min; i <= max; ++i)
		{
			//phonemeArray[i] = *(new Phoneme(i));
			phonemeArray[i].setCode(i);
			phonemeArray[i].initializeData();
			
		}
		threadsFinished++;
	}

	//Constructor
	MotuPlayer::MotuPlayer()
		:stream(0)
	{
		initializeData();
	}

	//Destructor
	MotuPlayer::~MotuPlayer()
	{
		delete[] phonemes;
		//free( phonemes);
	}

	//Open the stream
	bool MotuPlayer::openStream()
	{
		
		PaStreamParameters outputParameters;

		outputParameters.device = device;
		if (outputParameters.device == paNoDevice) {
			return false;
		}

		const PaDeviceInfo* pInfo = Pa_GetDeviceInfo(device);

		outputParameters.channelCount = channels;       
		outputParameters.sampleFormat = paFloat32; /* 32 bit floating point output */
		outputParameters.suggestedLatency = Pa_GetDeviceInfo(outputParameters.device)->defaultLowOutputLatency;
		outputParameters.hostApiSpecificStreamInfo = NULL;

		PaError err = Pa_OpenStream(
			&stream,
			NULL, /* no input */
			&outputParameters,
			SAMPLE_RATE,
			paFramesPerBufferUnspecified,
			paClipOff,      /* we won't output out of range samples so don't bother clipping them */
			&MotuPlayer::paCallback,
			this            /* Using 'this' for userData so we can cast to MotuPlayer* in paCallback method */
		);

		if (err != paNoError)
		{
			/* Failed to open stream to device !!! */
			return false;
		}

		err = Pa_SetStreamFinishedCallback(stream, &MotuPlayer::paStreamFinished);

		if (err != paNoError)
		{
			Pa_CloseStream(stream);
			stream = 0;

			return false;
		}

		return true;
	}

	//Close the stream
	bool MotuPlayer::closeStream()
	{
		
		if (stream == 0)
			return false;
		PaError err = Pa_CloseStream(stream);
		stream = 0;
		playing = false;
		return err == paNoError;
	}

	//Set a device index
	bool MotuPlayer::setDeviceIndex(PaDeviceIndex index)
	{
		device = index;
		return device != paNoDevice;
	}

	//Start a stream
	bool MotuPlayer::startStream()
	{
		if (stream == 0)
			return false;
		PaError err = Pa_StartStream(stream);
		playing = true;
		return err == paNoError;
	}

	//Stop a stream
	bool MotuPlayer::stopStream()
	{
		
		if (stream == 0)
			return false;

		PaError err = Pa_StopStream(stream);
		
		return err == paNoError;
	}

	//Initialize data
	void MotuPlayer::initializeData()
	{
		mode = sine;
		logCode = paNoError;
		playing = false;
		phoneme_row_table_index = 0;
		dynamic_table_height = 0;
		phoneme_index = -1;
		matrix_row_index = 0;
		channels = 24;
		use_motu = true;
		//phonemes = (Phoneme*)malloc(PHONEMES * sizeof(Phoneme));
		phonemes = new Phoneme[PHONEMES];
		threadsFinished = 0;
		int setSize = PHONEMES / INIT_THREADS;
		for (int k = 0; k < INIT_THREADS; ++k)
		{
			int min = k * (setSize + 1);
			int max = min + setSize > PHONEMES-1 ? PHONEMES-1 : min + setSize;
			std::thread phonemeInitThread(initializeSubsetOfPhonemes, phonemes, min, max);
			phonemeInitThread.detach();
		}

		int i;
		for (i = 0; i < SINE_TABLE_SIZE; i++)
		{
			sineData[i] = (float)(0.43*sin(((double)i / (double)SINE_TABLE_SIZE) * M_PI * 2.));
		}
		sineDataIndex = 0;

	}

	//Initialization finished
	bool MotuPlayer::initializationFinished()
	{
		return threadsFinished == INIT_THREADS;
		//return true;
	}


	//Get Motu Index
	bool MotuPlayer::setMOTU() {
		const PaDeviceInfo* info;
		int numDevices = Pa_GetDeviceCount();
		PaDeviceIndex motu = paNoDevice;
		std::string motuName("MOTU");
		for (int i = 0; i < numDevices; i++) {
			info = Pa_GetDeviceInfo(i);
			std::string deviceName(info->name);
			
			if (info->maxOutputChannels == 24 && deviceName.find(motuName) != std::string::npos)
			{
				motu = i;
				break;
			}
		}
		if (motu != paNoDevice)
			device = motu;
		return motu != paNoDevice;
		
	}

	//Restart control variables
	void MotuPlayer::restartControlVariables()
	{
		mode = sine;
		logCode = paNoError;
		playing = false;
		phoneme_row_table_index = 0;
		dynamic_table_height = 0;
		matrix_row_index = 0;
		phoneme_index = -1;
		channels = 24;
		use_motu = true;
	}

	//Instance port audio callback
	int MotuPlayer::paCallbackInstance(const void *inputBuffer, void *outputBuffer,
		unsigned long framesPerBuffer,
		const PaStreamCallbackTimeInfo* timeInfo,
		PaStreamCallbackFlags statusFlags)
	{
		float *out = (float*)outputBuffer;
		unsigned long i;

		(void)timeInfo; /* Prevent unused variable warnings. */
		(void)statusFlags;
		(void)inputBuffer;
		int k;
		for (i = 0; i < framesPerBuffer; i++)
		{
			switch (mode)
			{
			case sine:
				for (k = 0; k < channels; k++)
					*out++ = sineData[sineDataIndex];
				sineDataIndex += 1;
				if (sineDataIndex >= SINE_TABLE_SIZE) sineDataIndex -= SINE_TABLE_SIZE;
				break;
			case phoneme:
				for (k = 0; k < channels; k++)
					*out++ = phoneme_to_play->valueAt(phoneme_row_table_index, k);
				phoneme_row_table_index += 1;
				if (phoneme_row_table_index >= dynamic_table_height) {
					return paComplete;
				}
				break;
			case matrix:
				for (k = 0; k < channels; k++)
					*out++ = arbitraryMatrix[matrix_row_index*channels +k];
				matrix_row_index += 1;
				if (matrix_row_index >= dynamic_table_height) {
					return paComplete;
				}
				break;
			default:
				return paContinue;
			}
			
		}
		return paContinue;
	}

	//Instance stream finished callback
	void MotuPlayer::paStreamFinishedInstance()
	{
		restartControlVariables();
	}

	//Is playing
	bool MotuPlayer::isPlaying()
	{
		return playing;
	}

	//Set a phoneme index
	void  MotuPlayer::setPhonemeIndex(int phonemeToPlay)
	{
		phoneme_index = phonemeToPlay;
		phoneme_to_play = &phonemes[phoneme_index];
		//phoneme_to_play = new Phoneme(phonemeToPlay);
		dynamic_table_height = phoneme_to_play->getNumberOfRows();
	}

	//Change playmode
	void  MotuPlayer::changePlayMode(playModes newMode)
	{
		mode = newMode;
	}

	//Is the stream active
	bool MotuPlayer::isStreamActive()
	{
		return Pa_IsStreamActive(stream);
	}

	//Modify channels and dynamic table height to play an arbitrary matrix
	void MotuPlayer::setArbitraryMatrixParameters(float* matrix, int width, int height)
	{
		channels = width;
		dynamic_table_height = height;
		arbitraryMatrix = matrix;
		matrix_row_index = 0;
	}

	//Is using Motu as the playback device
	bool MotuPlayer::isUsingMotu()
	{
		return use_motu;
	}

	//Set the default output device as the playback device
	bool MotuPlayer::setDefaultOutput()
	{
		device = Pa_GetDefaultOutputDevice();
		if (device != paNoDevice)
		{
			const PaDeviceInfo* info = Pa_GetDeviceInfo(device);
			channels = info->maxOutputChannels;
			return true;
		}
		else
			return false;
	}

	//Use MOTU as the playback device
	void MotuPlayer::useMotu()
	{
		use_motu = true;
	}

	//use the default output as the playback device
	void MotuPlayer::useDefault()
	{
		use_motu = false;
	}


}