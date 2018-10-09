#pragma once
#include "Handlers.h"

#define DLLEXPORT _declspec(dllexport)

extern "C"
{
	//Create data structures
	DLLEXPORT void createStructures();

	//Plays a matrix that corresponds to the phoneme code
	DLLEXPORT int play(int phonemeCode);

	//Play a simple sine wave to test the connection in 24 channels
	DLLEXPORT int testPlay();

	//Returns log code
	DLLEXPORT int getLogCode();

	//Returns true if motu is playing a signal
	DLLEXPORT bool isMotuPlaying();

	//Play a specific matrix 
	DLLEXPORT int playMatrix(float* matrix, int width, int height);

	//Use motu as the playback device
	DLLEXPORT void useMotu();

	//Use the default device as the playback device
	DLLEXPORT void useDefaultOutput();
}



