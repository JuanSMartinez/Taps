#pragma once

#define SAMPLE_RATE   (44100)
#define FRAMES_PER_BUFFER (64)
#define SINE_TABLE_SIZE (200)
#define PHONEMES (49)

#ifndef M_PI
#define M_PI (3.14159265)
#endif

#include <portaudio.h>
#include <math.h>
#include <process.h>
#include <mutex>
#include <string>
#include "Phoneme.h"

namespace Handlers
{
	class MotuPlayer
	{
	public:

		//Play modes
		static enum playModes { sine, phoneme, matrix };

		//Constructor
		MotuPlayer();

		//Open the stream
		bool openStream();

		//close a stream
		bool closeStream();

		//Set the device index
		bool setDeviceIndex(PaDeviceIndex index);

		//Start the stream 
		bool startStream();

		//Stop the stream 
		bool stopStream();

		//is playing
		bool isPlaying();

		//Restart control variables
		void restartControlVariables();

		//Get the index device of motu and set it as a device
		bool setMOTU();

		//Set a phoneme index
		void setPhonemeIndex(int phonemeToPlay);

		//Change playmode
		void changePlayMode(playModes newMode);

		//Is the stream active
		bool isStreamActive();

		//Modify channels and dynamic table height to play an arbitrary matrix
		void setArbitraryMatrixParameters(float* matrix, int width, int height);

		//Is using motu as the playback device
		bool isUsingMotu();

		//Set the default outuput device as the playback device
		bool setDefaultOutput();

		//use motu as the playback device
		void useMotu();

		//use the default output as playback device
		void useDefault();

	private:
		playModes mode;
		PaStream *stream;
		int logCode;
		bool playing;
		int phoneme_row_table_index;
		int dynamic_table_height;
		int matrix_row_index;
		int phoneme_index;
		int channels;
		bool use_motu;
		PaDeviceIndex device;
		Phoneme* phoneme_to_play;
		float* arbitraryMatrix;
		Phoneme* phonemes[PHONEMES];

		float sineData[SINE_TABLE_SIZE];
		int sineDataIndex;
		
		//Initialize data 
		void initializeData();

		//Callback method for the instance used by port audio
		int paCallbackInstance(const void *inputBuffer, void *outputBuffer,
			unsigned long framesPerBuffer,
			const PaStreamCallbackTimeInfo* timeInfo,
			PaStreamCallbackFlags statusFlags);

		//Callback method  of the instance used when a stream finished
		void paStreamFinishedInstance();

		//Callback routine for when a stream is finished
		static void paStreamFinished(void* userData)
		{
			return ((MotuPlayer*)userData)->paStreamFinishedInstance();
		}

		//Callback routine of port audio
		static int paCallback(const void *inputBuffer, void *outputBuffer,
			unsigned long framesPerBuffer,
			const PaStreamCallbackTimeInfo* timeInfo,
			PaStreamCallbackFlags statusFlags,
			void *userData)
		{
			return((MotuPlayer*)userData)->paCallbackInstance(inputBuffer, outputBuffer,
				framesPerBuffer,
				timeInfo,
				statusFlags);
		}

	};

}