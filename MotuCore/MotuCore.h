#pragma once
#include "Handlers.h"

#define DLLEXPORT _declspec(dllexport)

typedef void(__stdcall * FinishedPlayingCallback)(int result);

extern "C"
{
	//Create data structures
	DLLEXPORT void createStructures();

	//Plays a matrix that corresponds to the phoneme code
	DLLEXPORT void play(int phonemeCode);

	//Play a simple sine wave to test the connection in 24 channels
	DLLEXPORT void testPlay();

	//Returns log code
	DLLEXPORT int getLogCode();

	//Returns true if motu is playing a signal
	DLLEXPORT int isMotuPlaying();

	//Play a specific matrix 
	DLLEXPORT void playMatrix(float* matrix, int width, int height);

	//Use motu as the playback device
	DLLEXPORT void useMotu();

	//Use the default device as the playback device
	DLLEXPORT void useDefaultOutput();

	//Set a finished playing callback
	DLLEXPORT void setFinishedPlayingCallback(FinishedPlayingCallback handler);

	//Initialization finished
	DLLEXPORT bool initializationFinished();

	//Clear all data
	DLLEXPORT void clearAll();
}



